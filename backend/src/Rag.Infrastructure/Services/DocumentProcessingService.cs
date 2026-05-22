using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Domain.Enums;
using Rag.Infrastructure.Persistence;
using Rag.Shared.Options;

namespace Rag.Infrastructure.Services;

public class DocumentProcessingService(
    ApplicationDbContext dbContext,
    ITextExtractionService extractionService,
    IChunkingService chunkingService,
    IEmbeddingService embeddingService,
    IOptions<ChunkingOptions> chunkOptions,
    ILogger<DocumentProcessingService> logger) : IDocumentProcessingService
{
    private readonly ChunkingOptions _chunkOptions = chunkOptions.Value;

    public async Task ProcessAsync(Guid documentId, string filePath, CancellationToken ct = default)
    {
        var document = await dbContext.Documents.FirstAsync(x => x.Id == documentId, ct);
        document.Status = DocumentStatus.Processing;
        await dbContext.SaveChangesAsync(ct);

        try
        {
            var ext = Path.GetExtension(filePath);
            var text = await extractionService.ExtractTextAsync(filePath, ext, ct);
            var chunks = chunkingService.ChunkText(text, _chunkOptions.DefaultChunkSize, _chunkOptions.DefaultOverlap);

            // Detect legal document type from content
            document.LegalDocumentType = DetectLegalDocumentType(text);

            // Create version record for initial processing
            var contentHash = ComputeContentHash(text);
            var version = new DocumentVersion
            {
                DocumentId = documentId,
                VersionNumber = document.CurrentVersion,
                ChangeDescription = "Initial document processing",
                FilePath = filePath,
                FileSizeBytes = document.FileSizeBytes,
                ContentHash = contentHash,
                IsCurrent = true
            };
            dbContext.Set<DocumentVersion>().Add(version);

            var chunkEntities = new List<DocumentChunk>();
            var index = 0;
            foreach (var chunk in chunks)
            {
                var embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Text, ct);
                chunkEntities.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    ChunkIndex = index++,
                    ChunkText = chunk.Text,
                    TokenCount = chunk.TokenCount,
                    EmbeddingJson = JsonSerializer.Serialize(embedding),
                    CreatedAt = DateTime.UtcNow,
                    SectionType = chunk.SectionType,
                    SectionNumber = chunk.SectionNumber,
                    SectionHeading = chunk.SectionHeading,
                    PageNumber = chunk.PageNumber,
                    CrossReferences = chunk.CrossReferences,
                    IsAmendment = chunk.IsAmendment,
                    VersionNumber = document.CurrentVersion
                });
            }

            dbContext.DocumentChunks.AddRange(chunkEntities);
            document.Status = DocumentStatus.Processed;
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Document processing failed for {DocumentId}", documentId);
            document.Status = DocumentStatus.Failed;
            document.ErrorMessage = ex.Message;
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private static LegalDocumentType DetectLegalDocumentType(string text)
    {
        var upper = text.ToUpperInvariant();

        // Check for contract indicators
        if (ContainsContractIndicators(upper))
            return LegalDocumentType.Contract;

        if (upper.Contains("AMENDMENT") && (upper.Contains("HEREBY AMENDED") || upper.Contains("IS AMENDED")))
            return LegalDocumentType.Amendment;

        if (upper.Contains("COURT") && (upper.Contains("OPINION") || upper.Contains("JUDGMENT") || upper.Contains("RULING")))
            return LegalDocumentType.CaseOpinion;

        if (upper.Contains("COURT") && (upper.Contains("ORDER") || upper.Contains("DECREE")))
            return LegalDocumentType.CourtOrder;

        if (upper.Contains("REGULATION") || upper.Contains("REGULATORY"))
            return LegalDocumentType.Regulation;

        if (upper.Contains("STATUTE") || upper.Contains("ACT OF") || upper.Contains("PUBLIC LAW"))
            return LegalDocumentType.Statute;

        if (upper.Contains("ORDINANCE"))
            return LegalDocumentType.Ordinance;

        if (upper.Contains("MEMORANDUM") && upper.Contains("LAW"))
            return LegalDocumentType.LegalMemo;

        if (upper.Contains("BRIEF") && (upper.Contains("PLAINTIFF") || upper.Contains("DEFENDANT") || upper.Contains("APPELLANT")))
            return LegalDocumentType.Brief;

        if (upper.Contains("COMPLAINT") || upper.Contains("PETITION") || upper.Contains("MOTION"))
            return LegalDocumentType.Pleading;

        if (upper.Contains("LEGISLATION") || upper.Contains("BILL"))
            return LegalDocumentType.Legislation;

        if (upper.Contains("POLICY"))
            return LegalDocumentType.PolicyDocument;

        return LegalDocumentType.Unknown;
    }

    private static bool ContainsContractIndicators(string upper)
    {
        var indicators = 0;
        if (upper.Contains("AGREEMENT") || upper.Contains("CONTRACT")) indicators++;
        if (upper.Contains("WHEREAS")) indicators++;
        if (upper.Contains("PARTIES") || upper.Contains("PARTY")) indicators++;
        if (upper.Contains("IN WITNESS WHEREOF")) indicators++;
        if (upper.Contains("HEREBY AGREE")) indicators++;
        return indicators >= 2;
    }

    private static string ComputeContentHash(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
