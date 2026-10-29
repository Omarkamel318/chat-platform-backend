using ChatPlatform.Application.CommonResponses;
using ChatPlatform.Application.DTOs.Messages;
using ChatPlatform.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<ActionResult<APIResponse<MessageDetailsResponse>>> SendMessage([FromBody] MessageRequest request)
        {
            MessageDetailsResponse result = await _messageService.SendMessageAsync(request);
            return Ok(APIResponse<MessageDetailsResponse>.Ok(result));
        }
    }
}
