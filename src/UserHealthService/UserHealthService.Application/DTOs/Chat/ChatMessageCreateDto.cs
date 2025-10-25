namespace UserHealthService.Application.DTOs.Chat
{
    public class ChatMessageCreateDto
    {
        public Guid ReceiverId { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? ParentMessageId { get; set; }
    }
}