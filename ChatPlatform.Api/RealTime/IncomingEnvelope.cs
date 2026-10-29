namespace ChatPlatform.Api.RealTime
{
    public class IncomingEnvelope
    {
        public string Type { get; set; } = string.Empty;
        public Guid? ChatRoomId { get; set; }
        public Guid? ReceiverId { get; set; }
        public string? Content { get; set; }
        public bool IsTyping { get; set; } = true;
    }
}
