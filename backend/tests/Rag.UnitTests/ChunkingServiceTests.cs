using Rag.Domain.Enums;
using Rag.Infrastructure.Services;

namespace Rag.UnitTests;

public class ChunkingServiceTests
{
    [Fact]
    public void ChunkText_ShouldCreateChunks_WhenTextIsLong()
    {
        var service = new ChunkingService();
        var text = string.Join(". ", Enumerable.Range(1, 20).Select(i => $"Sentence {i} with multiple words"));

        var result = service.ChunkText(text, 20, 5);

        Assert.NotEmpty(result);
        Assert.All(result, item => Assert.False(string.IsNullOrWhiteSpace(item.Text)));
    }

    [Fact]
    public void ChunkText_ShouldDetectArticles_WhenLegalStructurePresent()
    {
        var service = new ChunkingService();
        var text = "ARTICLE I. Definitions\nThis section defines key terms used throughout the agreement.\n\n" +
                   "ARTICLE II. Obligations\nThe parties shall perform their respective obligations as set forth herein.\n";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.SectionType == LegalSectionType.Article);
    }

    [Fact]
    public void ChunkText_ShouldDetectSections_WhenSectionHeadersPresent()
    {
        var service = new ChunkingService();
        var text = "SECTION 1.1. Scope of Work\nContractor shall provide services as described herein.\n\n" +
                   "SECTION 1.2. Payment Terms\nPayment shall be made within 30 days of invoice.\n";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.SectionType == LegalSectionType.Section);
    }

    [Fact]
    public void ChunkText_ShouldDetectWhereasClauses()
    {
        var service = new ChunkingService();
        var text = "WHEREAS, the Company desires to engage the Contractor for services.\n\n" +
                   "WHEREAS, the Contractor possesses the requisite skills and expertise.\n\n" +
                   "ARTICLE I. Agreement\nNow therefore the parties agree as follows.\n";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.SectionType == LegalSectionType.WhereasClause);
    }

    [Fact]
    public void ChunkText_ShouldDetectCrossReferences()
    {
        var service = new ChunkingService();
        var text = "SECTION 1. General\nAs defined in Section 2.1, the obligations pursuant to Article III shall apply.\n\n" +
                   "SECTION 2. Definitions\nTerms are defined herein.\n";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.CrossReferences != null);
    }

    [Fact]
    public void ChunkText_ShouldDetectAmendments()
    {
        var service = new ChunkingService();
        var text = "AMENDMENT 1. Revised Terms\nSection 3.2 is hereby amended by replacing the payment terms with new conditions.\n";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.SectionType == LegalSectionType.Amendment || c.IsAmendment);
    }

    [Fact]
    public void ChunkText_ShouldHandlePlainText_WhenNoLegalStructure()
    {
        var service = new ChunkingService();
        var text = "This is a simple document without any legal structure. It contains regular paragraphs of text.";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        Assert.All(result, c => Assert.Equal(LegalSectionType.Unknown, c.SectionType));
    }

    [Fact]
    public void ChunkText_ShouldRespectChunkSize_WhenSectionsAreLarge()
    {
        var service = new ChunkingService();
        var longSection = string.Join(". ", Enumerable.Range(1, 50).Select(i => $"Term number {i} means the definition value for item {i}"));
        var text = $"SECTION 1. Definitions\n{longSection}\n";

        var result = service.ChunkText(text, 20, 5);

        Assert.True(result.Count > 1, "Large sections should be split into multiple chunks");
        Assert.All(result, c => Assert.Equal(LegalSectionType.Section, c.SectionType));
    }

    [Fact]
    public void ChunkText_ShouldCaptureSectionNumbers()
    {
        var service = new ChunkingService();
        var text = "ARTICLE IV. Indemnification\nEach party shall indemnify the other against losses.\n";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        var articleChunk = result.First(c => c.SectionType == LegalSectionType.Article);
        Assert.NotNull(articleChunk.SectionNumber);
    }

    [Fact]
    public void ChunkText_ShouldPreservePreambleContent()
    {
        var service = new ChunkingService();
        var text = "This Agreement is entered into as of January 1, 2025 between Party A and Party B.\n\n" +
                   "ARTICLE I. Purpose\nThe purpose of this agreement is to establish terms.\n";

        var result = service.ChunkText(text, 500, 50);

        Assert.NotEmpty(result);
        Assert.Equal(LegalSectionType.Preamble, result.First().SectionType);
    }
}
