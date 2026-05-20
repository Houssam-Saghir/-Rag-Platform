using Rag.Domain.Common;

namespace Rag.Domain.Entities;

public class ChatSession : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? DocumentId { get; set; }
    public Document? Document { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
