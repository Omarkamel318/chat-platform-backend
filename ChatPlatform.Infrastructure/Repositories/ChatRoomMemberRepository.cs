using ChatPlatform.Application.IRepositories;
using ChatPlatform.Domain;
using ChatPlatform.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Infrastructure.Repositories
{
    public class ChatRoomMemberRepository : IChatRoomMemberRepository
    {
        private readonly ChatDbContext _context;
        private readonly DbSet<ChatRoomMember> _dbSet;

        public ChatRoomMemberRepository(ChatDbContext context)
        {
            _context = context;
            _dbSet = context.Set<ChatRoomMember>();
        }
         
        public async Task<ChatRoomMember?> GetMemberAsync(Guid chatRoomId, Guid userId)
            => await _dbSet
                    .Where(m => m.ChatRoomId == chatRoomId && m.UserId == userId)
                    .Where(ChatRoomMember.IsActiveExpression)
                    .Include(m=>m.User)
                    .FirstOrDefaultAsync();

        public async Task<IReadOnlyList<ChatRoomMember>> GetMembersForRoomAsync(Guid chatRoomId, IReadOnlySet<Guid> members)
        {
           return await _dbSet
                    .Where(m => m.ChatRoomId == chatRoomId && members.Contains(m.UserId))
                    .Where(ChatRoomMember.IsActiveExpression)
                    .Include(mem=>mem.User)
                    .ToListAsync();
        }
        public IReadOnlySet<Guid> GetMembersIdsForRoomAsync(Guid chatRoomId)
            =>  _dbSet
                    .Where(m=>m.ChatRoomId == chatRoomId)
                    .Where(ChatRoomMember.IsActiveExpression)
                    .Select(m=>m.Id)
                    .ToHashSet();

        public async Task<List<Guid>> GetMemberUserIdsAsync(Guid chatRoomId)
            => await _dbSet.Where(m=>m.ChatRoomId == chatRoomId)
                            .Where(ChatRoomMember.IsActiveExpression)
                            .Select(m=>m.UserId)
                            .ToListAsync();

        public async Task<bool> IsUserMemberAsync(Guid chatRoomId, Guid userId)
            => await _dbSet
                    .Where(m => m.ChatRoomId == chatRoomId && m.UserId == userId)
                    .Where(ChatRoomMember.IsActiveExpression)
                    .AnyAsync();

        public void AddMembers(Guid chatRoomId, IReadOnlyList<ChatRoomMember> members)
        {
            foreach (var member in members)
                member.ChatRoomId = chatRoomId;

            _dbSet.AddRange(members);
        }

        public void UpdateMemberAdminStatusAsync(ChatRoomMember member, bool isAdmin)
        {
            member.IsAdmin = isAdmin;
            _dbSet.Update(member);
        }

        //public async Task RemoveMembersAsync(Guid chatRoomId, IReadOnlyList<Guid> userIds)
        //{
        //    HashSet<Guid> temp = userIds.ToHashSet();
        //    List<ChatRoomMember> membersToRemove = await _dbSet
        //            .Where(m => m.ChatRoomId == chatRoomId && temp.Contains(m.UserId))
        //            .ToListAsync();

        //    _context.ChatRoomMembers.RemoveRange(membersToRemove);
        //}

        //public async Task UpdateReadStatusAsync(Guid chatRoomId, Guid userId, DateTime readAt)
        //{
        //    ChatRoomMember? member = await _dbSet
        //              .FirstOrDefaultAsync(m => m.ChatRoomId == chatRoomId && m.UserId == userId && m.IsActive);

        //    if (member is null)
        //        throw new KeyNotFoundException("Member not found in this room.");

        //    member.LastReadAt = readAt;
        //}

        public async Task<IReadOnlyList<ChatRoomMember>> GetMembersForRoomAsync(Guid chatRoomId)
            => await _dbSet
                    .Where(mem => mem.ChatRoomId == chatRoomId)
                    .Where(ChatRoomMember.IsActiveExpression)
                    .Include(mem=>mem.User)
                    .ToListAsync();

        public async Task SetClearedAtAsync(Guid chatRoomId, Guid userId, DateTime clearedAt)
        {
            ChatRoomMember? member = await GetMemberAsync(chatRoomId, userId);

            if (member is null)
                throw new KeyNotFoundException("Member not found in this room.");

            member.ClearedAt = clearedAt;
        }

        public async Task<ChatRoomMember?> GetMemberIncludingInactiveAsync(Guid chatRoomId, Guid userId)
            => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChatRoomId == chatRoomId && m.UserId == userId);

        public  IReadOnlySet<Guid> GetMemberUserIdsIncludingInactiveAsync(Guid chatRoomId)
            =>  _dbSet.Where(m => m.ChatRoomId == chatRoomId).Select(m => m.UserId).ToHashSet();

        public async Task<IReadOnlyList<ChatRoomMember>> SoftRemoveMembersAsync(Guid chatRoomId, IReadOnlyList<Guid> userIds, DateTime removedAt)
        {
            List<ChatRoomMember> members = await _dbSet
            .Where(m => m.ChatRoomId == chatRoomId && userIds.Contains(m.UserId))
            .Where(ChatRoomMember.IsActiveExpression)
            .ToListAsync();


            foreach (var member in members)
            {
                member.RemovedAt = removedAt;
                member.LeftAt = null;
                member.IsAdmin = false;
            }

            return members;
        }

        public async Task<ChatRoomMember?> MarkMemberLeftAsync(Guid chatRoomId, Guid userId, DateTime leftAt)
        {
            ChatRoomMember? member = await _context.ChatRoomMembers
                .Include(m=>m.User)
                .FirstOrDefaultAsync(m => m.ChatRoomId == chatRoomId && m.UserId == userId);

            if (member is null || !member.IsActive)
                return null;

            member.LeftAt = leftAt;
            member.RemovedAt = null;
            member.IsAdmin = false;

            return member;
        }
    }
}
