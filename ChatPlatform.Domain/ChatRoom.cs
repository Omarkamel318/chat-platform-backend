using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Domain
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public User CreatedBy { get; set; } = null!;
        public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
