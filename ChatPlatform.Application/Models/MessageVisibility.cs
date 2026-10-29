using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.Models
{
    public record MessageVisibility(DateTime? JoinedAt, DateTime? ClearedAt, DateTime? ExitedAt)
    {
        public static MessageVisibility From(ChatRoomMember member)
        {
            return new MessageVisibility(
                member.JoinedAt,
                member.ClearedAt,
                member.RemovedAt ?? member.LeftAt
                );
        }
    }
}
