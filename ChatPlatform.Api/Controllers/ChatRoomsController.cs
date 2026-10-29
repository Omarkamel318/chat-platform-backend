using ChatPlatform.Application.CommonResponses;
using ChatPlatform.Application.DTOs.ChatRoom;
using ChatPlatform.Application.DTOs.ChatRoomMember;
using ChatPlatform.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomsController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;

        public ChatRoomsController(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<ChatRoomResponse>>>> GetRoomsForUser(Guid userId)
        {
            IReadOnlyList<ChatRoomResponse> result = await _chatRoomService.GetRoomsForUserAsync(userId);
            return Ok(APIResponse<IReadOnlyList<ChatRoomResponse>>.Ok(result));
        }

        [HttpGet("{roomId:guid}")]
        public async Task<ActionResult<APIResponse<ChatRoomDetailsResponse>>> GetRoomDetails(
        Guid roomId,
        [FromQuery] Guid requestingUserId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 30)
        {
            ChatRoomDetailsResponse result = await _chatRoomService.GetRoomDetailsAsync(roomId, requestingUserId, skip, take);
            return Ok(APIResponse<ChatRoomDetailsResponse>.Ok(result));
        }

        [HttpGet("direct-room/{userId:guid}")]
        public async Task<ActionResult<APIResponse<ChatRoomDetailsResponse>>> GetOrCreateDirectRoom(
       Guid userId,
       [FromQuery] Guid otherUserId
       )
        {
            ChatRoomDetailsResponse result = await _chatRoomService.GetOrCreateDirectRoomAsync(userId, otherUserId);
            return Ok(APIResponse<ChatRoomDetailsResponse>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<APIResponse<ChatRoomResponse>>> CreateRoom([FromBody] ChatRoomRequest request)
        {
            ChatRoomResponse result = await _chatRoomService.CreateRoomAsync(request);
            return CreatedAtAction(nameof(GetRoomDetails),
                new { roomId = result.Id,
                    requestingUserId = request.CreatedByUserId }, APIResponse<ChatRoomResponse>.Ok(result));
        }

        [HttpGet("{roomId:guid}/members")]
        public async Task<ActionResult<APIResponse<RoomMembersResponse>>> GetRoomMembers(Guid roomId, [FromQuery] Guid requestingUserId)
        {
            var result = await _chatRoomService.GetRoomMembersAsync(roomId, requestingUserId);
            return Ok(APIResponse<RoomMembersResponse>.Ok(result));
        }

        [HttpPost("members")]
        public async Task<IActionResult> AddMembers([FromBody] AddRemoveMembersRequest request)
        {
            await _chatRoomService.AddMembersAsync(request);
            return NoContent();
        }

        [HttpDelete("members")]
        public async Task<IActionResult> RemoveMembers([FromBody] AddRemoveMembersRequest request)
        {
            await _chatRoomService.RemoveMembersAsync(request);
            return NoContent();
        }

        [HttpPut("members/admin-status")]
        public async Task<IActionResult> UpdateMemberAdminStatus([FromBody] UpdateMemberAdminRequest request)
        {
            await _chatRoomService.UpdateMemberAdminStatusAsync(request);
            return NoContent();
        }

        [HttpPut("{roomId:guid}/close")]
        public async Task<IActionResult> CloseRoom(Guid roomId, [FromQuery] Guid requestingUserId)
        {
            await _chatRoomService.CloseRoomAsync(roomId, requestingUserId);
            return NoContent();
        }

        [HttpPut("{roomId:guid}/reopen")]
        public async Task<IActionResult> ReopenRoom(Guid roomId, [FromQuery] Guid requestingUserId)
        {
            await _chatRoomService.ReopenRoomAsync(roomId, requestingUserId);
            return NoContent();
        }

        [HttpDelete("{roomId:guid}/clear")]
        public async Task<IActionResult> ClearRoomForMe(Guid roomId, [FromQuery] Guid userId)
        {
            await _chatRoomService.ClearRoomForUserAsync(roomId, userId);
            return NoContent();
        }

        [HttpPost("{roomId:guid}/leave")]
        public async Task<IActionResult> LeaveRoom(Guid roomId, [FromQuery] Guid userId)
        {
            await _chatRoomService.LeaveRoomAsync(roomId, userId);
            return NoContent();
        }
    }
}
