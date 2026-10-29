using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.ChatRoomMember
{
    public class UpdateMemberAdminRequest
    {
        public Guid ChatRoomId { get; set; }
        public Guid RequestingUserId { get; set; }
        public Guid TargetUserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
