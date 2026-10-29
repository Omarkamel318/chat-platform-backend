using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.Messages
{
    public class MessageResponse
    {
        public string SenderUserName { get; set; } = string.Empty;
        public Guid SenderUserId { get; set; } 
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;

    }
}
