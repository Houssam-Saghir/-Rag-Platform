using Rag.Domain.Enums;

namespace Rag.Application.DTOs;

public record DocumentDto(
    Guid Id,
    string OriginalFileName,
    string FileType,
    DocumentStatus Status,
    DateTime UploadedAt,
    long FileSizeBytes,
    LegalDocumentType LegalDocumentType,
    string? Jurisdiction,
    string? PracticeArea,
    string? CaseNumber,
    string? MatterNumber,
    DateTime? EffectiveDate,
    DateTime? ExpirationDate,
    int CurrentVersion);

public record UploadDocumentResponse(Guid DocumentId, string Message);
