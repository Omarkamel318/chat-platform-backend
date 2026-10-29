using ChatPlatform.Application.CommonResponses;
using ChatPlatform.Application.DTOs.Users;
using ChatPlatform.Application.IServices;
using ChatPlatform.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse<IReadOnlyList<UserResponse>>>> GetAll()
        {
            IReadOnlyList<UserResponse> result = await _userService.GetAllUsersAsync();
            return Ok(APIResponse<IReadOnlyList<UserResponse>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<APIResponse<UserResponse>>> GetUser(Guid id)
        {
            UserResponse? result = await _userService.GetUserAsync(id);
            return Ok(APIResponse<UserResponse>.Ok(result));
        }

        [HttpGet("{chatRoomId:guid}/candidate-users")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<CandidateUserResponse>>>> GetCandidateUsersForRoom(Guid chatRoomId)
        {
            IReadOnlyList<CandidateUserResponse> result = await _userService.GetCandidateUsersForRoomAsync(chatRoomId);
            return Ok(APIResponse<IReadOnlyList<CandidateUserResponse>>.Ok(result));
        }



    }
}
