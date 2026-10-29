using ChatPlatform.Application.DTOs.Messages;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.ChatRoom
{
    public class ChatRoomResponse
    {
        public Guid Id { get; set; }
        public Guid? OtherDirectUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? LastReadAtForRequestedUser { get; set; }
        public MessageResponse? LastMessage { get; set; }
    }
}
