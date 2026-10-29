using ChatPlatform.Application;
using ChatPlatform.Application.IRepositories;
using ChatPlatform.Infrastructure.Contexts;
using ChatPlatform.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ChatDbContext _context;

        public IChatRoomRepository chatRooms;

        public IChatRoomMemberRepository chatRoomMembers;

        public IUserRepository users;

        public IMessageRepository messages;

        public UnitOfWork(ChatDbContext context)
        {
            _context = context;
        }
        public IChatRoomRepository ChatRooms => chatRooms ??= new ChatRoomRepository(_context);

        public IChatRoomMemberRepository ChatRoomMembers => chatRoomMembers ??= new ChatRoomMemberRepository(_context);

        public IUserRepository Users => users ??= new UserRepository(_context);

        public IMessageRepository Messages => messages ??= new MessageRepository(_context);

        public Task<int> SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}
