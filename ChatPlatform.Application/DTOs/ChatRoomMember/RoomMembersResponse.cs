using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.ChatRoomMember
{
    public class RoomMembersResponse
    {
        public Guid CreatedByUserId { get; set; }
        public IReadOnlyList<ChatRoomMemberResponse> Members { get; set; } = new List<ChatRoomMemberResponse>();
    }
}
