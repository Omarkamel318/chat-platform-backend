using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.ChatRoomMember
{
    public class ChatRoomMemberResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
