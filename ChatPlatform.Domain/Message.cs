namespace ChatPlatform.Domain
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ChatRoomId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; } = null;
        public ChatRoom ChatRoom { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
}