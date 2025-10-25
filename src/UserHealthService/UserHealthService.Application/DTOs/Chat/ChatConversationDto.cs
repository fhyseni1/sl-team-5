namespace UserHealthService.Application.DTOs.Chat
{
    public class ChatConversationDto
    {
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsDoctor { get; set; }
    }
}