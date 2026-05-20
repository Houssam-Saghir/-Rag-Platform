using Rag.Domain.Enums;

namespace Rag.Application.DTOs;

public record DocumentDto(Guid Id, string OriginalFileName, string FileType, DocumentStatus Status, DateTime UploadedAt, long FileSizeBytes);
public record UploadDocumentResponse(Guid DocumentId, string Message);
