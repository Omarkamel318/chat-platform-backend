using ChatPlatform.Application.DTOs.Users;
using ChatPlatform.Application.IServices;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChatPlatform.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;

        public UserService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<UserResponse> CreateUserAsync(string name)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentException("Name is required");

            User user = new User
            {
                Id = Guid.NewGuid(),
                UserName = name
            };

            await _uow.Users.AddUserAsync(user);
            await _uow.SaveChangesAsync();

            return new UserResponse
            {
                UserId = user.Id,
                UserName = user.UserName
            };
        }

        public async Task DeleteUserAsync(Guid id)
        {
            User? user = await _uow.Users.GetByIdAsync(id);

            if (user is null)
                throw new ArgumentException("User is not exist");

            _uow.Users.DeleteUser(user);
            await _uow.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync()
              => (await _uow.Users.GetAllUsersAsync()).Select(u => new UserResponse
                    {
                        UserId = u.Id,
                        UserName = u.UserName,
                 }).ToList();

        public async Task<IReadOnlyList<CandidateUserResponse>> GetCandidateUsersForRoomAsync(Guid chatRoomId)
        {
            IReadOnlySet<Guid> existingUsers =  _uow.ChatRoomMembers.GetMemberUserIdsIncludingInactiveAsync(chatRoomId);
            return (await _uow.Users.GetAllUsersAsync()).Select(u => new CandidateUserResponse
            {
                UserId = u.Id,
                UserName = u.UserName,
                IsCandidate = !existingUsers.Contains(u.Id)
            }).ToList(); 
        }

        public async Task<UserResponse?> GetUserAsync(Guid id)
        {
            User? user = await _uow.Users.GetByIdAsync(id);

            if (user is null)
                return null;
            

            return new UserResponse
            {
                UserId = user.Id,
                UserName = user.UserName
            };
        }
        public async Task<UserResponse> UpdateUserAsync(Guid id, string name)
        {
            User? user = await _uow.Users.GetByIdAsync(id);

            if (user is null)
                throw new ArgumentException("User is not exist");

            user.UserName = name;

            _uow.Users.UpdateUser(user);
            await _uow.SaveChangesAsync();

            return new UserResponse
            {
                UserId = user.Id,
                UserName = user.UserName
            };
        }
    }

}
