using ChatPlatform.Application.Models;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IRepositories
{
    public interface IChatRoomRepository
    {
        Task<ChatRoom?> GetByIdAsync(Guid id);
        Task<ChatRoom?> GetRoomWithMembersAsync(Guid roomId);
        Task<ChatRoom?> GetRoomWithSpecificMemberAndUnreadMessagesAsync(Guid roomId,Guid userId);
        Task<ChatRoom?> GetRoomWithDetailsAsync(Guid id, MessageVisibility visibility, int messagesSkip, int messagesTake);
        Task<IReadOnlyList<ChatRoom>> GetRoomsForUserAsync(Guid userId);
        Task<ChatRoom?> FindDirectRoomAsync(Guid userId1, Guid userId2);
        Task<bool> IsRoomClosedAsync(Guid chatRoomId);
        Task AddAsync(ChatRoom chatRoom);
        void Update(ChatRoom chatRoom);
    }
}
