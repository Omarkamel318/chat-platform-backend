using System.Linq.Expressions;

namespace ChatPlatform.Domain
{
    public class ChatRoomMember
    {
        public Guid Id { get; set; }
        public Guid ChatRoomId { get; set; }
        public Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LastReadAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public DateTime? LeftAt { get; set; }


        public ChatRoom ChatRoom { get; set; } = null!;
        public User User { get; set; } = null!;

        public static readonly Expression<Func<ChatRoomMember, bool>> IsActiveExpression =
            m => (m.RemovedAt == null && m.LeftAt == null);


        public static readonly Func<ChatRoomMember, bool> IsActiveCompiled = IsActiveExpression.Compile();
        public bool IsActive => IsActiveCompiled(this);
    }
}