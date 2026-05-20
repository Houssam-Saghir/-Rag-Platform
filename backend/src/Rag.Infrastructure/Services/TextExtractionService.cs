using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using Rag.Application.Exceptions;
using Rag.Application.Interfaces;

namespace Rag.Infrastructure.Services;

public class TextExtractionService(ILogger<TextExtractionService> logger) : ITextExtractionService
{
    public async Task<string> ExtractTextAsync(string filePath, string extension, CancellationToken ct = default)
    {
        logger.LogInformation("Extracting text from {FilePath}", filePath);
        return extension.ToLowerInvariant() switch
        {
            ".txt" => await File.ReadAllTextAsync(filePath, ct),
            ".pdf" => ExtractPdf(filePath),
            ".docx" => ExtractDocx(filePath),
            _ => throw new ValidationException("Unsupported file type")
        };
    }

    private static string ExtractPdf(string filePath)
    {
        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(filePath);
        foreach (var page in doc.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }

    private static string ExtractDocx(string filePath)
    {
        using var doc = WordprocessingDocument.Open(filePath, false);
        return doc.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
    }
}
