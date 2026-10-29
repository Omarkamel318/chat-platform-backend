using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IRepositories
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        void UpdateUser(User user);
        void DeleteUser(User user);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<IReadOnlyList<User>> GetExceptAllUsersWithExistingUsers(Guid chatRoomId, IReadOnlySet<Guid> existingUsers);
    }
}
