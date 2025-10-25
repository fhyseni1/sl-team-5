namespace UserHealthService.Application.DTOs.Chat
{
    public class ChatMessageResponseDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public Guid ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public Guid? ParentMessageId { get; set; }
        public string? ParentMessage { get; set; }
        public DateTime SentAt { get; set; }
        public List<ChatMessageResponseDto> Replies { get; set; } = new();
    }
}