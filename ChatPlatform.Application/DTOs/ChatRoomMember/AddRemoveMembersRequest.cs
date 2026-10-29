using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.ChatRoomMember
{
    public class AddRemoveMembersRequest
    {
        public Guid ChatRoomId { get; set; }
        public Guid RequestingUserId { get; set; }
        public IReadOnlyList<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
