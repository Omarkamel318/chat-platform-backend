using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IRepositories
{
    public interface IChatRoomMemberRepository
    {
        Task<bool> IsUserMemberAsync(Guid chatRoomId, Guid userId);
        Task<ChatRoomMember?> GetMemberIncludingInactiveAsync(Guid chatRoomId, Guid userId); //
        Task<ChatRoomMember?> GetMemberAsync(Guid chatRoomId, Guid userId);
        IReadOnlySet<Guid> GetMembersIdsForRoomAsync(Guid chatRoomId);
        Task<List<Guid>> GetMemberUserIdsAsync(Guid chatRoomId);
        IReadOnlySet<Guid> GetMemberUserIdsIncludingInactiveAsync(Guid chatRoomId);
        Task<IReadOnlyList<ChatRoomMember>> GetMembersForRoomAsync(Guid chatRoomId, IReadOnlySet<Guid> members);
        void AddMembers(Guid chatRoomId, IReadOnlyList<ChatRoomMember> members);

        //Task RemoveMembersAsync(Guid chatRoomId, IReadOnlyList<Guid> userIds);
        Task<IReadOnlyList<ChatRoomMember>> SoftRemoveMembersAsync(Guid chatRoomId, IReadOnlyList<Guid> userIds, DateTime removedAt);
        void UpdateMemberAdminStatusAsync(ChatRoomMember member, bool isAdmin);
        //Task UpdateReadStatusAsync(Guid chatRoomId, Guid userId, DateTime readAt);
        Task<IReadOnlyList<ChatRoomMember>> GetMembersForRoomAsync(Guid chatRoomId);
        Task SetClearedAtAsync(Guid chatRoomId, Guid userId, DateTime clearedAt);
        //Task<DateTime?> GetClearedAtAsync(Guid chatRoomId, Guid userId);
        Task<ChatRoomMember?> MarkMemberLeftAsync(Guid chatRoomId, Guid userId, DateTime leftAt);

    }
}
