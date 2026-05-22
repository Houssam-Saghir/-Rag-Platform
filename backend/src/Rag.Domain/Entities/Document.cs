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

    // Law-specific metadata
    public LegalDocumentType LegalDocumentType { get; set; } = LegalDocumentType.Unknown;
    public string? Jurisdiction { get; set; }
    public string? PracticeArea { get; set; }
    public string? CaseNumber { get; set; }
    public string? MatterNumber { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Parties { get; set; }

    // Versioning
    public int CurrentVersion { get; set; } = 1;
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
