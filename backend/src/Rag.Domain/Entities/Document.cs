using Rag.Domain.Common;
using Rag.Domain.Enums;

namespace Rag.Domain.Entities;

public class Document : BaseAuditableEntity, ISoftDelete
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public string? ErrorMessage { get; set; }
    public Guid UploadedByUserId { get; set; }
    public User UploadedBy { get; set; } = null!;
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
