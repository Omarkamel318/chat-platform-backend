using ChatPlatform.Application.DTOs.Messages;
using ChatPlatform.Application.IRepositories;
using ChatPlatform.Application.IServices;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealtimeNotifier _notifier;

        public MessageService(IUnitOfWork uow, IRealtimeNotifier notifier)
        {
            _uow = uow;
            _notifier = notifier;
        }
        public async Task<MessageDetailsResponse> SendMessageAsync(MessageRequest request)
        {
            Guid chatRoomId;

            if (request.ChatRoomId.HasValue)
            {
                bool isMember = await _uow.ChatRoomMembers.IsUserMemberAsync(request.ChatRoomId.Value, request.SenderId); //h:  room may be not exist
                if (!isMember)
                    throw new UnauthorizedAccessException("User is not a member of this chat room.");

                chatRoomId = request.ChatRoomId.Value;
            }
            else if (request.ReceiverId.HasValue)
            {
                ChatRoom? existingRoom = await _uow.ChatRooms.FindDirectRoomAsync(request.SenderId, request.ReceiverId.Value);

                if (existingRoom is not null)
                {
                    chatRoomId = existingRoom.Id;
                }
                else
                {
                    ChatRoom newRoom = new ChatRoom
                    {
                        Id = Guid.NewGuid(),
                        Name = string.Empty,
                        IsGroup = false,
                        CreatedByUserId = request.SenderId,
                        CreatedAt = DateTime.Now,
                        Members = new List<ChatRoomMember>
                {
                    new ChatRoomMember { Id = Guid.NewGuid(), UserId = request.SenderId, IsAdmin = true, JoinedAt = DateTime.Now },
                    new ChatRoomMember { Id = Guid.NewGuid(), UserId = request.ReceiverId.Value, IsAdmin = true, JoinedAt = DateTime.Now }
                }
                    };

                    await _uow.ChatRooms.AddAsync(newRoom);
                    await _uow.SaveChangesAsync();
                    chatRoomId = newRoom.Id;
                }
            }
            else
            {
                throw new ArgumentException("Either ChatRoomId or ReceiverId must be provided.");
            }


            bool isClosed = await _uow.ChatRooms.IsRoomClosedAsync(chatRoomId);
            if (isClosed)
                throw new InvalidOperationException("This room is closed. You cannot send new messages.");


            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                SenderId = request.SenderId,
                Content = request.Content,
                SentAt = DateTime.Now,
            };
            await _uow.Messages.AddAsync(message);
            await _uow.SaveChangesAsync();

            User? sender = await _uow.Users.GetByIdAsync(request.SenderId);

            MessageDetailsResponse messageResponse = new MessageDetailsResponse
            {
                chatRoomId = chatRoomId,
                senderUserName = sender.UserName,
                content = message.Content,
                sentAt = message.SentAt,
                senderId = request.SenderId,
                readBy = new List<MessageReadStatusResponse>()
            };

            IReadOnlyList<Guid> userIds = await _uow.ChatRoomMembers.GetMemberUserIdsAsync(chatRoomId);

            await _notifier.NotifyNewMessageAsync(userIds, messageResponse);

            return messageResponse;
        }
    }
}
