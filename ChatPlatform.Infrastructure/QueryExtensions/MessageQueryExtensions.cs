using ChatPlatform.Application.Models;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Infrastructure.QueryExtensions
{
    public static class MessageQueryExtensions
    {
        public static IQueryable<Message> ApplyVisibility(this IQueryable<Message> query, MessageVisibility visibility)
        {
            var clearedAt = visibility.ClearedAt;
            var exitedAt = visibility.ExitedAt;
            var joinedAt = visibility.JoinedAt;

            return query.Where(m =>
                (m.SentAt >= joinedAt)
                &&
                (clearedAt == null || m.SentAt > clearedAt)
                && 
                (exitedAt == null || m.SentAt <= exitedAt));
        }
    }
}
