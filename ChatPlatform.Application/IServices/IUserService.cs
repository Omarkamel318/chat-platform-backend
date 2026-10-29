using ChatPlatform.Application.DTOs.Users;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IServices
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserResponse>> GetAllUsersAsync();
        Task<IReadOnlyList<CandidateUserResponse>> GetCandidateUsersForRoomAsync(Guid chatRoomId);
        Task<UserResponse?> GetUserAsync(Guid id);
        Task<UserResponse> CreateUserAsync(string name);
        Task<UserResponse> UpdateUserAsync(Guid id, string name);
        Task DeleteUserAsync(Guid id);
    }
}
