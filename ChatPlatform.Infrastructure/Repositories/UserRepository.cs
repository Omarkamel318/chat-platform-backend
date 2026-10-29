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
    public class UserRepository : IUserRepository
    {
        private readonly ChatDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(ChatDbContext context)
        {
            _context = context;
            _dbSet = context.Set<User>();
        }

        public async Task AddUserAsync(User user)
        {
            await _dbSet.AddAsync(user);
        }

        public void DeleteUser(User user)
        {
            _dbSet.Remove(user);
        }

        public async Task<bool> ExistsAsync(Guid id)
            =>await _dbSet.AnyAsync(u=>u.Id == id);

        public async Task<IReadOnlyList<User>> GetAllUsersAsync()
            => await _dbSet.OrderBy(u=>u.UserName).ToListAsync();

        public async Task<User?> GetByIdAsync(Guid id)
            =>await _dbSet.FindAsync(id);

        public async Task<IReadOnlyList<User>> GetExceptAllUsersWithExistingUsers(Guid chatRoomId, IReadOnlySet<Guid> existingUsers)
            => await _dbSet.Where(u => !existingUsers.Contains(u.Id)).ToListAsync();

        public void UpdateUser(User user)
            => _dbSet.Update(user);
    }
}
