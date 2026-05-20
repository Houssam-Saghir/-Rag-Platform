using Microsoft.AspNetCore.Http;
using Rag.Application.DTOs;

namespace Rag.Application.Interfaces;

public interface IDocumentService
{
    Task<UploadDocumentResponse> UploadAsync(Guid userId, IFormFile file, CancellationToken ct = default);
    Task<IReadOnlyCollection<DocumentDto>> GetMyDocumentsAsync(Guid userId, CancellationToken ct = default);
    Task<DocumentDto> GetByIdAsync(Guid userId, Guid documentId, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid documentId, CancellationToken ct = default);
}
