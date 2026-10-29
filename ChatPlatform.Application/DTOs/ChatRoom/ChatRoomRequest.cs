using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.ChatRoom
{
    public class ChatRoomRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; }
        public IReadOnlyList<Guid>? MemberUserIds { get; set; } = new List<Guid>();
    }
}
