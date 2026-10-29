namespace ChatPlatform.Application.DTOs.Messages
{
    public class MessageReadStatusResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime ReadAt { get; set; }
    }
}