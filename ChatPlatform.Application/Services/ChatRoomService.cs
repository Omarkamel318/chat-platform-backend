using ChatPlatform.Application.DTOs.ChatRoom;
using ChatPlatform.Application.DTOs.ChatRoomMember;
using ChatPlatform.Application.DTOs.Messages;
using ChatPlatform.Application.IRepositories;
using ChatPlatform.Application.IServices;
using ChatPlatform.Application.Models;
using ChatPlatform.Domain;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.Services
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealtimeNotifier _notifier;

        public ChatRoomService(IUnitOfWork uow, IRealtimeNotifier notifier)
        {
            _uow = uow;
            _notifier = notifier;
        }

        public async Task<ChatRoomResponse> CreateRoomAsync(ChatRoomRequest request)
        {
            ChatRoom room = new ChatRoom
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                IsGroup = true,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.Now,
            };

            room.Members.Add(new ChatRoomMember
            {
                Id = Guid.NewGuid(),
                UserId = request.CreatedByUserId,
                IsAdmin = true,
                JoinedAt = DateTime.Now
            });


            foreach (Guid userId in request.MemberUserIds!.Where(id => id != request.CreatedByUserId))
            {
                room.Members.Add(new ChatRoomMember
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsAdmin = false,
                    JoinedAt = DateTime.Now
                });
            }

            await _uow.ChatRooms.AddAsync(room);
            await _uow.SaveChangesAsync();

            return new ChatRoomResponse
            {
                Id = room.Id,
                Name = room.Name,
                IsGroup = room.IsGroup,
            };
        }

        public async Task<ChatRoomDetailsResponse> GetOrCreateDirectRoomAsync(Guid requestingUserId, Guid otherUserId)
        {
            ChatRoom? room = await _uow.ChatRooms.FindDirectRoomAsync(requestingUserId, otherUserId);

            if (room is null)
            {
                room = new ChatRoom
                {
                    Id = Guid.NewGuid(),
                    Name = string.Empty,
                    IsGroup = false,
                    CreatedByUserId = requestingUserId,
                    CreatedAt = DateTime.Now,
                    Members = new List<ChatRoomMember>
                    {
                    new ChatRoomMember { Id = Guid.NewGuid(), UserId = otherUserId, IsAdmin = true, JoinedAt = DateTime.Now },
                    new ChatRoomMember { Id = Guid.NewGuid(), UserId = requestingUserId, IsAdmin = true, JoinedAt = DateTime.Now }
                }
                };

                await _uow.ChatRooms.AddAsync(room);
                await _uow.SaveChangesAsync();
            }

            //IReadOnlyList<ChatRoomMember> members = await _uow.ChatRoomMembers.GetAllMembersForRoomAsync(room.Id);

            return new ChatRoomDetailsResponse
            {
                Id = room.Id,
                Name = room.IsGroup ? room.Name : room.Members.FirstOrDefault(m => m.UserId != requestingUserId)!.User.UserName,
                IsGroup = room.IsGroup,
                CreatedByUserId = room.CreatedByUserId,
                CreatedAt = room.CreatedAt,
                ClosedAt = room.ClosedAt,
            };
        }

        public async Task<IReadOnlyList<ChatRoomResponse>> GetRoomsForUserAsync(Guid userId)
        {
            IReadOnlyList<ChatRoom> rooms = await _uow.ChatRooms.GetRoomsForUserAsync(userId);

            return rooms.Select(r =>
            {

                Message? lastMessage = r.Messages.FirstOrDefault();

                User? otherDirectUser = !r.IsGroup ? r.Members.FirstOrDefault(m => m.UserId != userId).User : null;


                ChatRoomMember? userAsMember = r.Members.FirstOrDefault(m => m.UserId == userId);

                return new ChatRoomResponse
                {
                    Id = r.Id,
                    Name = r.IsGroup ? r.Name : otherDirectUser!.UserName,
                    IsGroup = r.IsGroup,
                    CreatedAt = r.CreatedAt,
                    ClosedAt = r.ClosedAt,
                    OtherDirectUserId = r.IsGroup ? null : otherDirectUser!.Id ,
                    LastReadAtForRequestedUser = userAsMember!.LastReadAt,
                    LastMessage = lastMessage == null ? null : new MessageResponse
                    {
                        SenderUserId = lastMessage.Sender.Id,
                        SenderUserName = lastMessage.Sender.UserName,
                        Content = lastMessage.Content,
                        SentAt = lastMessage.SentAt,
                        
                    }
                };
            }
                ).ToList();
        }

        public async Task<ChatRoomDetailsResponse> GetRoomDetailsAsync(Guid roomId, Guid requestingUserId, int msgSkip, int msgTake)
        {
            ChatRoomMember? viewer = await _uow.ChatRoomMembers.GetMemberIncludingInactiveAsync(roomId, requestingUserId);

            if (viewer is null)
                throw new UnauthorizedAccessException("You are not a member of this room.");

            MessageVisibility visibility = MessageVisibility.From(viewer);

            ChatRoom? room = await _uow.ChatRooms.GetRoomWithDetailsAsync(roomId, visibility, msgSkip, msgTake);


            if (room is null)
                throw new KeyNotFoundException("Room not found.");

            //IReadOnlyList<ChatRoomMember> members = await _uow.ChatRoomMembers.GetAllMembersForRoomAsync(roomId);


            return new ChatRoomDetailsResponse 
            {
                Id = room.Id,
                Name = room.IsGroup ? room.Name : room.Members.FirstOrDefault(m=>m.UserId != requestingUserId)!.User.UserName,
                IsGroup = room.IsGroup,
                CreatedByUserId = room.CreatedByUserId,
                CreatedAt = room.CreatedAt,
                ClosedAt = room.ClosedAt,
                CurrentMemberIsActive = viewer.IsActive,
                Messages = room.Messages.OrderBy(m => m.SentAt).Select(m => new MessageDetailsResponse
                {
                    chatRoomId = room.Id,
                    senderUserName = m.Sender.UserName,
                    senderId = m .SenderId,
                    content = m.Content,
                    sentAt = m.SentAt,
                    readBy = m.SenderId == requestingUserId 
                               ? room.Members
                    .Where(mem => mem.UserId != m.SenderId
                        && mem.LastReadAt.HasValue
                        && mem.LastReadAt.Value >= m.SentAt)
                    .Select(mem => new MessageReadStatusResponse
                    {
                        UserId = mem.UserId,
                        UserName = mem.User.UserName,
                        ReadAt = room.IsGroup ? mem.LastReadAt!.Value : m.ReadAt!.Value
                    }).ToList()
                : null
        }).ToList()
            };
        }

        public async Task AddMembersAsync(AddRemoveMembersRequest request)
        {
            //check group, member is admin, 
            await validateGroupRoomRequest(request.ChatRoomId, request.RequestingUserId, "add members");

            IReadOnlySet<Guid> existingMembers = _uow.ChatRoomMembers.GetMemberUserIdsIncludingInactiveAsync(request.ChatRoomId); 

            HashSet<Guid> newRequestedUserIds = new HashSet<Guid>();

            foreach (Guid memberId in request.UserIds)
            {
                if(!newRequestedUserIds.Contains(memberId))
                    newRequestedUserIds.Add(memberId);
            }

            IReadOnlyList<ChatRoomMember> newRequestedMembers = newRequestedUserIds
                .Where(ui=>!existingMembers.Contains(ui))
                .Select(userId => new ChatRoomMember
            {
                Id = Guid.NewGuid(),
                ChatRoomId = request.ChatRoomId,
                UserId = userId,
                IsAdmin = false,
                JoinedAt = DateTime.Now
            }).ToList();

            _uow.ChatRoomMembers.AddMembers(request.ChatRoomId, newRequestedMembers);

            await _uow.SaveChangesAsync();

            
            if (newRequestedMembers.Count == 0)
                return;

            var activeIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(request.ChatRoomId);

            var newMembers = await _uow.ChatRoomMembers.GetMembersForRoomAsync(request.ChatRoomId, newRequestedUserIds);

            var newMembersResponse = newMembers.OrderBy(m=>m.JoinedAt).Select(m => new ChatRoomMemberResponse
            {
                UserId = m.UserId,
                UserName = m.User.UserName,
                JoinedAt = m.JoinedAt,
                IsAdmin = m.IsAdmin
            }).ToList();

            await _notifier.NotifyMemberAddedAsync(activeIds, request.ChatRoomId, newMembersResponse);
        }

        public async Task RemoveMembersAsync(AddRemoveMembersRequest request)
        {
            //check group, member is admin
            (ChatRoomMember requestingMember, ChatRoom room) = await validateGroupRoomRequest(request.ChatRoomId, request.RequestingUserId, "remove members");

            if (requestingMember.UserId != room.CreatedByUserId) // not owner
            {
                IReadOnlyList<ChatRoomMember> removeMembers = await _uow.ChatRoomMembers.GetMembersForRoomAsync(request.ChatRoomId, request.UserIds.ToHashSet());
                foreach (ChatRoomMember member in removeMembers)
                {
                    if (member.IsAdmin)
                    {
                        throw new UnauthorizedAccessException("Only the group creator can remove the admins");
                    }
                }

            }
            IReadOnlyList<ChatRoomMember> removedMembers = await _uow.ChatRoomMembers.SoftRemoveMembersAsync(request.ChatRoomId, request.UserIds, DateTime.Now);
            await _uow.SaveChangesAsync();

            if (removedMembers.Count == 0)
                return;

            var activeIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(request.ChatRoomId);


            await _notifier.NotifyMemberRemovedAsync(
                removedMembers.Select(m=>m.UserId).ToList(), 
                request.ChatRoomId, 
                true,
                new List<Guid>());

            await _notifier.NotifyMemberRemovedAsync(activeIds,
                request.ChatRoomId,
                false,
                removedMembers.Select(m=>m.UserId).ToList());
        }

        public async Task UpdateMemberAdminStatusAsync(UpdateMemberAdminRequest request)
        {
            (ChatRoomMember requestingMember, ChatRoom room) = await validateGroupRoomRequest(request.ChatRoomId, request.RequestingUserId,"change admin status");

            if (requestingMember.UserId != room.CreatedByUserId)
                throw new UnauthorizedAccessException("Only the group creator can change admin status");

            ChatRoomMember? targetMember = await _uow.ChatRoomMembers.GetMemberAsync(request.ChatRoomId, request.TargetUserId);
            if (targetMember is null || !targetMember.IsActive)
                throw new KeyNotFoundException("Target user is not a member of this room.");

            _uow.ChatRoomMembers.UpdateMemberAdminStatusAsync(targetMember, request.IsAdmin);
            await _uow.SaveChangesAsync();

            var activeIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(request.ChatRoomId);

            await _notifier.NotifyUpdateMemberAdminStatusAsync(activeIds,
                request.ChatRoomId,
                new ChatRoomMemberResponse
                {
                    UserId = targetMember.UserId,
                    UserName = targetMember.User.UserName,
                    JoinedAt = targetMember.JoinedAt,
                    IsAdmin = targetMember.IsAdmin
                });

        }

        public async Task MarkAsReadAsync(Guid chatRoomId, Guid userId)
        {
            //bool isMember = await _uow.ChatRoomMembers.IsUserMemberAsync(chatRoomId, userId);
            //if (!isMember)
            //    throw new UnauthorizedAccessException("You are not a member of this room.");

            DateTime readAt = DateTime.Now;

            //update LastReadAt for member or throw exception is userid is not member

            ChatRoom? testRoom = await _uow.ChatRooms.GetRoomWithSpecificMemberAndUnreadMessagesAsync(chatRoomId, userId);


            if (testRoom is null)
                throw new KeyNotFoundException("Room not found.");

            ChatRoomMember? userMember = testRoom.Members.FirstOrDefault();

            if (userMember is null || !userMember.IsActive)
                throw new KeyNotFoundException("Member not found in this room.");

            userMember.LastReadAt = readAt;


            if (!testRoom.IsGroup)
            {
                foreach (Message message in testRoom.Messages)
                {
                    message.ReadAt = readAt;
                }

            }

            await _uow.SaveChangesAsync();

            IReadOnlyList<Guid> recipientIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(chatRoomId);
                                                        
            await _notifier.NotifyReadReceiptAsync(recipientIds, chatRoomId, userId, userMember.User.UserName, readAt);

        }

        public async Task NotifyTypingAsync(Guid chatRoomId, Guid userId, bool isTyping)
        {
            bool isMember = await _uow.ChatRoomMembers.IsUserMemberAsync(chatRoomId,userId);
            if(!isMember)
                throw new UnauthorizedAccessException("You are not a member of this room.");

            IReadOnlyList<Guid> recipientIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(chatRoomId);
                                                        

                    
            await _notifier.NotifyTypingAsync(recipientIds, chatRoomId, userId, isTyping);
        }

        public async Task<RoomMembersResponse> GetRoomMembersAsync(Guid roomId, Guid requestingUserId)
        {
            bool isMember = await _uow.ChatRoomMembers.IsUserMemberAsync(roomId, requestingUserId);
            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this room.");

            ChatRoom? result = await _uow.ChatRooms.GetRoomWithMembersAsync(roomId);

            return new RoomMembersResponse
            {
                CreatedByUserId = result.CreatedByUserId,
                Members = result.Members.Where(m=>m.IsActive).Select(member => new ChatRoomMemberResponse
                {
                    UserName = member.User.UserName,
                    UserId = member.User.Id,
                    IsAdmin = member.IsAdmin,
                    JoinedAt = member.JoinedAt
                }).ToList()
            };
        }

        public async Task CloseRoomAsync(Guid roomId, Guid requestingUserId)
        {
            ChatRoom? room = await _uow.ChatRooms.GetByIdAsync(roomId);
            if (room is null)
                throw new KeyNotFoundException("Room not found.");

            if (!room.IsGroup)
                throw new InvalidOperationException("Only group rooms can be closed.");

            if (room.CreatedByUserId != requestingUserId)
                throw new UnauthorizedAccessException("Only the room owner can close this room.");

            DateTime closedAt = DateTime.Now;

            room.ClosedAt = closedAt;
            _uow.ChatRooms.Update(room);
            await _uow.SaveChangesAsync();

            IReadOnlyList<Guid> otherMemberIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(roomId);

            await _notifier.NotifyRoomClosedAsync(otherMemberIds, roomId, closedAt);

        }

        public async Task ReopenRoomAsync(Guid roomId, Guid requestingUserId)
        {
            ChatRoom? room = await _uow.ChatRooms.GetByIdAsync(roomId);

            if (room is null)
                throw new KeyNotFoundException("Room not found.");

            if (room.CreatedByUserId != requestingUserId)
                throw new UnauthorizedAccessException("Only the room owner can reopen this room.");

            room.ClosedAt = null;
            _uow.ChatRooms.Update(room);
            await _uow.SaveChangesAsync();

            IReadOnlyList<Guid> otherMemberIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(roomId);

            await _notifier.NotifyRoomReopenedAsync(otherMemberIds, roomId);
        }

        public async Task ClearRoomForUserAsync(Guid roomId, Guid userId)
        {
            await _uow.ChatRoomMembers.SetClearedAtAsync(roomId, userId,DateTime.Now);
            await _uow.SaveChangesAsync();
        }

        public async Task LeaveRoomAsync(Guid roomId, Guid userId)
        {
            ChatRoom? room = await _uow.ChatRooms.GetByIdAsync(roomId);

            if (room is null)
                throw new KeyNotFoundException("Room not found.");

            if (!room.IsGroup)
                throw new InvalidOperationException("Only group rooms can be left.");

            if (room.CreatedByUserId == userId)
                throw new InvalidOperationException("The owner cannot leave the group.");

            ChatRoomMember? member = await _uow.ChatRoomMembers.MarkMemberLeftAsync(roomId, userId, DateTime.Now);

            if (member is null)
                throw new UnauthorizedAccessException("You are not a member of this room.");

            // get activeIds before member left to notify for LeavedMember
            var activeIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(roomId);

            await _uow.SaveChangesAsync();


            await _notifier.NotifyMemberLeftAsync(activeIds,
                roomId,
                new ChatRoomMemberResponse
                {
                    UserId = member.UserId,
                    UserName = member.User.UserName,
                    JoinedAt = member.JoinedAt,
                    IsAdmin = member.IsAdmin
                });
        }

        private async Task<(ChatRoomMember reqMember, ChatRoom room)> validateGroupRoomRequest(Guid chatRoomId, Guid requestingUserId, string oper)
        {
            ChatRoom? room = await _uow.ChatRooms.GetByIdAsync(chatRoomId);

            if (room is null)
                throw new KeyNotFoundException("Room not found.");

            if (!room.IsGroup)
                throw new InvalidOperationException($"Room is not group to {oper}");

            if (room.ClosedAt.HasValue)
                throw new InvalidOperationException($"Can't {oper} for closed room");

            ChatRoomMember? requestingMember = await _uow.ChatRoomMembers.GetMemberAsync(chatRoomId, requestingUserId);

            if (requestingMember is null || !requestingMember.IsAdmin || !requestingMember.IsActive)
                throw new UnauthorizedAccessException($"Only active admins can {oper}.");

            return (requestingMember, room);
        }

    }
}



/*
 result.Select(member => new ChatRoomMemberResponse
            {
                UserName = member.User.UserName,
                UserId = member.User.Id,
                IsAdmin = member.IsAdmin,
                JoinedAt = member.JoinedAt
            }).ToList();
 
 */