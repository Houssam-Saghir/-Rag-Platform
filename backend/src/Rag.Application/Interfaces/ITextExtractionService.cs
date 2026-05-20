namespace Rag.Application.Interfaces;

public interface ITextExtractionService
{
    Task<string> ExtractTextAsync(string filePath, string extension, CancellationToken ct = default);
}
