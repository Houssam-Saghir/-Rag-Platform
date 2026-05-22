using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rag.Application.DTOs;
using Rag.Application.Exceptions;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Domain.Enums;
using Rag.Infrastructure.Persistence;

namespace Rag.Infrastructure.Services;

public class DocumentService(
    ApplicationDbContext dbContext,
    IFileStorageService fileStorageService,
    IDocumentProcessingService processingService,
    ILogger<DocumentService> logger) : IDocumentService
{
    public async Task<UploadDocumentResponse> UploadAsync(Guid userId, IFormFile file, CancellationToken ct = default)
    {
        var document = new Document
        {
            UploadedByUserId = userId,
            OriginalFileName = Path.GetFileName(file.FileName),
            FileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName),
            FileType = Path.GetExtension(file.FileName).ToLowerInvariant(),
            FileSizeBytes = file.Length,
            Status = DocumentStatus.Pending
        };

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(ct);

        var filePath = await fileStorageService.SaveAsync(userId, document.Id, file, ct);
        _ = Task.Run(() => processingService.ProcessAsync(document.Id, filePath, CancellationToken.None), CancellationToken.None);
        logger.LogInformation("Document {DocumentId} uploaded by user {UserId}", document.Id, userId);
        return new UploadDocumentResponse(document.Id, "Document uploaded and queued for processing");
    }

    public async Task<IReadOnlyCollection<DocumentDto>> GetMyDocumentsAsync(Guid userId, CancellationToken ct = default)
        => await dbContext.Documents
            .Where(x => x.UploadedByUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DocumentDto(x.Id, x.OriginalFileName, x.FileType, x.Status, x.CreatedAt, x.FileSizeBytes,
                x.LegalDocumentType, x.Jurisdiction, x.PracticeArea, x.CaseNumber, x.MatterNumber,
                x.EffectiveDate, x.ExpirationDate, x.CurrentVersion))
            .ToListAsync(ct);

    public async Task<DocumentDto> GetByIdAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        var item = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == documentId && x.UploadedByUserId == userId, ct)
            ?? throw new NotFoundException("Document not found");

        return new DocumentDto(item.Id, item.OriginalFileName, item.FileType, item.Status, item.CreatedAt, item.FileSizeBytes,
            item.LegalDocumentType, item.Jurisdiction, item.PracticeArea, item.CaseNumber, item.MatterNumber,
            item.EffectiveDate, item.ExpirationDate, item.CurrentVersion);
    }

    public async Task DeleteAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        var item = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == documentId && x.UploadedByUserId == userId, ct)
            ?? throw new NotFoundException("Document not found");

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
    }
}
