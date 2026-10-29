using ChatPlatform.Application.IRepositories;
using ChatPlatform.Application.Models;
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
    public class ChatRoomRepository : IChatRoomRepository
    {
        private readonly ChatDbContext _context;
        private readonly DbSet<ChatRoom> _dbSet;

        public ChatRoomRepository(ChatDbContext context)
        {
            _context = context;
            _dbSet = context.Set<ChatRoom>();
        }

        public async Task AddAsync(ChatRoom chatRoom)
            => await _dbSet.AddAsync(chatRoom);

        public async Task<ChatRoom?> FindDirectRoomAsync(Guid userId1, Guid userId2)
            => await _dbSet
                    .Where(r => !r.IsGroup)
                    .Where(r => r.Members.Any(m => m.UserId == userId1))
                    .Where(r => r.Members.Any(m => m.UserId == userId2))
                    .Include(r=>r.Members)
                    .FirstOrDefaultAsync();

        public async Task<ChatRoom?> GetByIdAsync(Guid id)
            => await _dbSet.FindAsync(id);


        public async Task<IReadOnlyList<ChatRoom>> GetRoomsForUserAsync(Guid userId)
        {
            //IReadOnlyList<ChatRoom> rooms = await _dbSet
            //        .Where(r => r.Members.Any(m => m.UserId == userId))
            //        .Include(r=>r.Members.Where(m => m.UserId == userId || !m.ChatRoom.IsGroup))
            //            .ThenInclude(m=>m.User)
            //        .Include(r => r.Messages
            //                        .OrderByDescending(m => m.SentAt)
            //                        .Take(1)) //h: trying to Eliminating خقيثق
            //            .ThenInclude(m => m.Sender)
            //        .AsNoTracking()
            //        .ToListAsync();

            //var visibleRooms = rooms.Where(r =>
            //{
            //    var lastMessage = r.Messages.FirstOrDefault();
            //    var clearedAt = r.Members.FirstOrDefault()?.ClearedAt;

            //    if (clearedAt is null || lastMessage is null)
            //        return true;

            //    return  lastMessage.SentAt > clearedAt.Value;
            //});

            IReadOnlyList<ChatRoom> rooms = await _context.ChatRoomMembers
                .Where(mem => mem.UserId == userId)
                .Select(mem => new ChatRoom
                {
                    Id = mem.ChatRoomId,
                    Name = mem.ChatRoom.Name,
                    ClosedAt = mem.ChatRoom.ClosedAt,
                    CreatedByUserId = mem.ChatRoom.CreatedByUserId,
                    IsGroup = mem.ChatRoom.IsGroup,
                    CreatedAt = mem.ChatRoom.CreatedAt,

                    Members = mem.ChatRoom.Members
                        .Where(m => m.UserId == userId || !m.ChatRoom.IsGroup)
                        .Select(m=>new ChatRoomMember
                        {
                            Id = m.Id,
                            User = m.User,
                            ChatRoomId = m.ChatRoomId,
                            IsAdmin = m.IsAdmin,
                            JoinedAt = m.JoinedAt,
                            ClearedAt = m.ClearedAt,
                            LeftAt = m.LeftAt,
                            LastReadAt = m.LastReadAt,
                            RemovedAt = m.RemovedAt,
                            UserId = m.UserId
                        })
                        .ToList(),



                    Messages = mem.ChatRoom.Messages
                        .Where(m =>
                                            (mem.ClearedAt == null || m.SentAt > mem.ClearedAt)
                                                &&
                                            ((mem.RemovedAt ?? mem.LeftAt) == null || m.SentAt <= (mem.RemovedAt ?? mem.LeftAt))
                                                &&
                                            (m.SentAt >= mem.JoinedAt))
                        .OrderByDescending(m => m.SentAt)
                        .Take(1)
                        .Select(msg=>new Message
                        {
                            Id =msg.Id,
                            Content = msg.Content,
                            Sender = msg.Sender,
                            SentAt = msg.SentAt,
                            ReadAt = msg.ReadAt
                        })
                        .ToList()
                        
                }).ToListAsync();


            return rooms
                .OrderByDescending(r => r.Messages.FirstOrDefault() != null? r.Messages.First().SentAt : r.CreatedAt)
                .ToList();

        }

        public async Task<ChatRoom?> GetRoomWithDetailsAsync(Guid id, MessageVisibility visibility, int messagesSkip, int messagesTake)
            => await _context.ChatRooms
                     .Include(r => r.Messages
                                            .Where(m =>
                                            (m.SentAt >= visibility.JoinedAt)
                                                &&
                                            (visibility.ClearedAt == null || m.SentAt > visibility.ClearedAt)
                                                &&
                                            (visibility.ExitedAt == null || m.SentAt <= visibility.ExitedAt)
                                                )
                                            .OrderByDescending(m => m.SentAt)
                                            .Skip(messagesSkip)
                                            .Take(messagesTake))
                         .ThenInclude(m => m.Sender)
                     .Include(r => r.Members.Where(m => m.RemovedAt == null && m.LeftAt == null))
                         .ThenInclude(m=>m.User)
                     .AsNoTracking()
                     .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<ChatRoom?> GetRoomWithMembersAsync(Guid roomId)
            => await _dbSet.Include(r => r.Members.OrderBy(m => m.JoinedAt))
                                .ThenInclude(m => m.User)
                            .AsNoTracking() 
                            .FirstOrDefaultAsync(r => r.Id == roomId);

        public async Task<ChatRoom?> GetRoomWithSpecificMemberAndUnreadMessagesAsync(Guid roomId, Guid userId)
         => await _dbSet
                .Where(r => r.Id == roomId)
                .Include(r => r.Members.Where(m => m.UserId == userId))
                    .ThenInclude(m=>m.User)
                .Include(r => r.Messages.Where(m => !m.ChatRoom.IsGroup && !m.ReadAt.HasValue))
                .FirstOrDefaultAsync();
                
                        

        public async Task<bool> IsRoomClosedAsync(Guid chatRoomId)
            => await _dbSet.Where(r => r.Id == chatRoomId)
            .Select(r => r.ClosedAt.HasValue)
            .FirstOrDefaultAsync();

        public void Update(ChatRoom chatRoom)
            =>_context.Update(chatRoom);
    }
}
