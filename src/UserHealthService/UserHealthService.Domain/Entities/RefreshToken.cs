// UserHealthService.Domain/Entities/RefreshToken.cs
namespace UserHealthService.Domain.Entities
{
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsRevoked { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}