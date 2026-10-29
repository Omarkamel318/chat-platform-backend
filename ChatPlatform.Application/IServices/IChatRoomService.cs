using ChatPlatform.Application.DTOs.ChatRoom;
using ChatPlatform.Application.DTOs.ChatRoomMember;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IServices
{
    public interface IChatRoomService
    {
        Task<IReadOnlyList<ChatRoomResponse>> GetRoomsForUserAsync(Guid userId);
        Task<ChatRoomDetailsResponse> GetRoomDetailsAsync(Guid roomId, Guid requestingUserId, int msgSkip, int msgTake);
        Task<RoomMembersResponse> GetRoomMembersAsync(Guid roomId, Guid requestingUserId);
        Task<ChatRoomResponse> CreateRoomAsync(ChatRoomRequest request);
        Task AddMembersAsync(AddRemoveMembersRequest request);
        Task RemoveMembersAsync(AddRemoveMembersRequest request);
        Task UpdateMemberAdminStatusAsync(UpdateMemberAdminRequest request);
        Task MarkAsReadAsync(Guid chatRoomId, Guid userId);
        Task NotifyTypingAsync(Guid chatRoomId, Guid userId, bool isTyping);
        Task CloseRoomAsync(Guid roomId, Guid requestingUserId);
        Task ReopenRoomAsync(Guid roomId, Guid requestingUserId);
        Task ClearRoomForUserAsync(Guid roomId, Guid userId);
        Task<ChatRoomDetailsResponse> GetOrCreateDirectRoomAsync(Guid requestingUserId, Guid otherUserId);
        Task LeaveRoomAsync(Guid roomId, Guid userId);
    }
}
