using ChatPlatform.Application.IRepositories;
using ChatPlatform.Application.Models;
using ChatPlatform.Domain;
using ChatPlatform.Infrastructure.Contexts;
using ChatPlatform.Infrastructure.QueryExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;
        private readonly DbSet<Message> _dbSet;

        public MessageRepository(ChatDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Message>();
        }

        public async Task AddAsync(Message message)
            => await _dbSet.AddAsync(message);

        public async Task<IReadOnlyList<Message>> GetRoomMessagesAsync(Guid chatRoomId, MessageVisibility visibility, int skip, int take)
            =>await _dbSet
                        .Where(m => m.ChatRoomId == chatRoomId)
                        .ApplyVisibility(visibility)
                        .Include(m => m.Sender)
                        .OrderByDescending(m => m.SentAt)
                        .Skip(skip)
                        .Take(take)
                        .AsNoTracking()
                        .ToListAsync();

        public async Task<IReadOnlyList<Message>> GetUnReadMessagesForUserAsync(Guid chatRoomId, Guid userId)
            => await _context.Messages
            .Where(m => m.ChatRoomId == chatRoomId
                && m.SenderId != userId
                && m.ReadAt == null)
            .ToListAsync();

        
    }
}
