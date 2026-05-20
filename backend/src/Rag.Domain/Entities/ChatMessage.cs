using Rag.Domain.Common;
using Rag.Domain.Enums;

namespace Rag.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatSessionId { get; set; }
    public ChatSession ChatSession { get; set; } = null!;
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? SourceChunkIdsJson { get; set; }
}
