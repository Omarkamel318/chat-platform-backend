using ChatPlatform.Application.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application
{
    public interface IUnitOfWork
    {
        public IChatRoomRepository ChatRooms { get; }
        public IChatRoomMemberRepository ChatRoomMembers { get; }
        public IUserRepository Users { get; }
        public IMessageRepository Messages { get; }
        Task<int> SaveChangesAsync();
    }
}
