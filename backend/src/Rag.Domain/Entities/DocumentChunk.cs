using Rag.Domain.Common;
using Rag.Domain.Enums;

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

    // Law-specific chunk metadata
    public LegalSectionType SectionType { get; set; } = LegalSectionType.Unknown;
    public string? SectionNumber { get; set; }
    public string? SectionHeading { get; set; }
    public int? PageNumber { get; set; }
    public string? CrossReferences { get; set; }
    public bool IsAmendment { get; set; }
    public int VersionNumber { get; set; } = 1;
}
