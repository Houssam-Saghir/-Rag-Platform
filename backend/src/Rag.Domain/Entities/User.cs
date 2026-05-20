using Rag.Domain.Common;

namespace Rag.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
