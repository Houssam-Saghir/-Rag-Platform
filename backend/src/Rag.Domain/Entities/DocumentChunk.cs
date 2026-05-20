using Rag.Domain.Common;

namespace Rag.Domain.Entities;

public class DocumentChunk : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string ChunkText { get; set; } = string.Empty;
    public string EmbeddingJson { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int TokenCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
