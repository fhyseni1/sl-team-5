// ChatMessage.cs
namespace UserHealthService.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public Guid? ParentMessageId { get; set; } // For replies
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public virtual User Sender { get; set; } = null!;
        public virtual User Receiver { get; set; } = null!;
        public virtual ChatMessage? ParentMessage { get; set; }
        public virtual ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
    }
}