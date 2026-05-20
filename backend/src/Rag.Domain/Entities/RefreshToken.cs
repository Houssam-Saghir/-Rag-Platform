using Rag.Domain.Common;

namespace Rag.Domain.Entities;

public class RefreshToken : BaseAuditableEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
