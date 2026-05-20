using Rag.Domain.Entities;

namespace Rag.Application.Interfaces;

public interface IVectorSearchService
{
    Task<IReadOnlyCollection<(DocumentChunk Chunk, float Score)>> SearchAsync(Guid documentId, string question, int topK, CancellationToken ct = default);
}
