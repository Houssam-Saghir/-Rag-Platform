using Rag.Domain.Common;

namespace Rag.Domain.Entities;

public class DocumentVersion : BaseAuditableEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public int VersionNumber { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
