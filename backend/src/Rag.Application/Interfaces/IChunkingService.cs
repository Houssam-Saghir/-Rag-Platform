using Rag.Domain.Enums;

namespace Rag.Application.Interfaces;

public interface IChunkingService
{
    IReadOnlyCollection<LegalChunkResult> ChunkText(string text, int chunkSize, int overlap);
}

public record LegalChunkResult(
    string Text,
    int TokenCount,
    LegalSectionType SectionType,
    string? SectionNumber,
    string? SectionHeading,
    int? PageNumber,
    string? CrossReferences,
    bool IsAmendment);
