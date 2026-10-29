using ChatPlatform.Application.DTOs.ChatRoomMember;
using ChatPlatform.Application.DTOs.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.ChatRoom
{
    public class ChatRoomDetailsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool CurrentMemberIsActive { get; set; }

        public IReadOnlyList<MessageDetailsResponse> Messages { get; set; } = new List<MessageDetailsResponse>();
    }
}
