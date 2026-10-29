using ChatPlatform.Application.DTOs.ChatRoomMember;
using ChatPlatform.Application.DTOs.Messages;
using ChatPlatform.Application.IServices;
using ChatPlatform.Domain;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ChatPlatform.Api.RealTime
{
    public class WebSocketNotifier : IRealtimeNotifier
    {
        private readonly ConnectionManager _connectionManager;

        public WebSocketNotifier(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public Task NotifyMemberAddedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, IReadOnlyList<ChatRoomMemberResponse> newMembers)
        {
            return SendToUsersAsync(recipientUserIds, new { type = "member_added", data = new { chatRoomId, newMembers } });

        }

        public Task NotifyUpdateMemberAdminStatusAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, ChatRoomMemberResponse member)
        {
            return SendToUsersAsync(recipientUserIds, new { type = "member_updated", data = new { chatRoomId, member } });
        }

        public Task NotifyMemberLeftAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, ChatRoomMemberResponse member)
        {
            return SendToUsersAsync(recipientUserIds, new { type = "member_left", data = new { chatRoomId, member } });
        }

        public Task NotifyMemberRemovedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, bool isRemoved, IReadOnlyList<Guid> removedMembersUserIds)
        {
            return SendToUsersAsync(recipientUserIds, new { type = "member_removed", data = new { chatRoomId, isRemoved, removedMembersUserIds } });
        }

        public async Task NotifyNewMessageAsync(IReadOnlyList<Guid> recipientUserIds, MessageDetailsResponse message)
        {
            await SendToUsersAsync(recipientUserIds, new { type = "new_message", data = message });
        }

        public Task NotifyReadReceiptAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, Guid readerUserId, string readerUserName, DateTime readAt)
        {
            return SendToUsersAsync(recipientUserIds, new
            {
                type = "read_receipt",
                data = new { chatRoomId, readerUserId, readerUserName, readAt }
            });
        }

        public Task NotifyRoomClosedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, DateTime closedAt)
        {
            return SendToUsersAsync(recipientUserIds, new
            {
                type = "room_closed",
                data = new { chatRoomId, closedAt }
            });
        }

        public Task NotifyRoomReopenedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId)
        {
            return SendToUsersAsync(recipientUserIds, new
            {
                type = "room_reopened",
                data = new { chatRoomId }
            });
        }

        public Task NotifyTypingAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, Guid typingUserId, bool isTyping)
        {
            return SendToUsersAsync(recipientUserIds, new
            {
                type = "typing",
                data = new { chatRoomId, typingUserId, isTyping }
            });

        }

        private async Task SendToUsersAsync(IReadOnlyList<Guid> userIds, object envelope)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

            foreach (Guid userId in userIds)
            {
                foreach (ManagedSocket managedSocket in _connectionManager.GetConnections(userId))
                {
                    if (managedSocket.Socket.State == WebSocketState.Open)
                    {
                        await managedSocket.SendLock.WaitAsync();
                        try
                        {
                            await managedSocket.Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, default);
                        }
                        finally
                        {
                            managedSocket.SendLock.Release();
                        }
                    }
                }
            }
        }
    }
}
