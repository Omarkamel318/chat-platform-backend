using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.Messages
{
    public class MessageDetailsResponse
    {
        public Guid chatRoomId { get; set; }
        public Guid senderId { get; set; }
        public string senderUserName { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public DateTime sentAt { get; set; } = DateTime.Now;
        public IReadOnlyList<MessageReadStatusResponse>? readBy { get; set; }

    }
}
