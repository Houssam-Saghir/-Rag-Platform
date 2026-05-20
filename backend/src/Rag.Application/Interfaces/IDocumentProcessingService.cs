namespace Rag.Application.Interfaces;

public interface IDocumentProcessingService
{
    Task ProcessAsync(Guid documentId, string filePath, CancellationToken ct = default);
}
