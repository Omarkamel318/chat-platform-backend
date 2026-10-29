using ChatPlatform.Application.DTOs.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IServices
{
    public interface IMessageService
    {
        Task<MessageDetailsResponse> SendMessageAsync(MessageRequest request);
    }
}
