using ChatPlatform.Application.DTOs.ChatRoomMember;
using ChatPlatform.Application.DTOs.Messages;
using ChatPlatform.Domain;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IServices
{
    public interface IRealtimeNotifier
    {
        Task NotifyNewMessageAsync(IReadOnlyList<Guid> recipientUserIds, MessageDetailsResponse message);
        Task NotifyReadReceiptAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, Guid readerUserId, string readerUserName, DateTime readAt);
        Task NotifyTypingAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, Guid typingUserId, bool isTyping);
        Task NotifyRoomClosedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, DateTime closedAt);
        Task NotifyRoomReopenedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId);
        Task NotifyMemberAddedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, IReadOnlyList<ChatRoomMemberResponse> newMembers);
        Task NotifyMemberRemovedAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, bool isRemoved, IReadOnlyList<Guid> removedMembersUserIds);
        Task NotifyMemberLeftAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, ChatRoomMemberResponse member);
        Task NotifyUpdateMemberAdminStatusAsync(IReadOnlyList<Guid> recipientUserIds, Guid chatRoomId, ChatRoomMemberResponse member);
    }
}
