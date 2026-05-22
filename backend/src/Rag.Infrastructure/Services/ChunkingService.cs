using System.Text.RegularExpressions;
using Rag.Application.Interfaces;
using Rag.Domain.Enums;

namespace Rag.Infrastructure.Services;

public partial class ChunkingService : IChunkingService
{
    // Legal section patterns ordered by specificity
    private static readonly (Regex Pattern, LegalSectionType Type)[] SectionPatterns =
    [
        (ArticleRegex(), LegalSectionType.Article),
        (SectionRegex(), LegalSectionType.Section),
        (ClauseRegex(), LegalSectionType.Clause),
        (SubClauseRegex(), LegalSectionType.SubClause),
        (WhereasRegex(), LegalSectionType.WhereasClause),
        (RecitalRegex(), LegalSectionType.Recital),
        (DefinitionRegex(), LegalSectionType.Definition),
        (ScheduleRegex(), LegalSectionType.Schedule),
        (ExhibitRegex(), LegalSectionType.Exhibit),
        (AppendixRegex(), LegalSectionType.Appendix),
        (AmendmentRegex(), LegalSectionType.Amendment),
        (PreambleRegex(), LegalSectionType.Preamble),
        (SignatureRegex(), LegalSectionType.Signature)
    ];

    private static readonly Regex CrossRefRegex = CrossReferenceRegex();
    private static readonly Regex PageBreakRegex = PageBreakPattern();

    public IReadOnlyCollection<LegalChunkResult> ChunkText(string text, int chunkSize, int overlap)
    {
        var sections = SplitIntoLegalSections(text);
        var chunks = new List<LegalChunkResult>();

        foreach (var section in sections)
        {
            var sectionChunks = ChunkSection(section, chunkSize, overlap);
            chunks.AddRange(sectionChunks);
        }

        return chunks;
    }

    private static List<LegalSection> SplitIntoLegalSections(string text)
    {
        var sections = new List<LegalSection>();

        // Track page numbers via form-feed characters or page markers
        var pageBreaks = PageBreakRegex.Matches(text);
        var pageMap = BuildPageMap(text, pageBreaks);

        // Find all section boundaries
        var boundaries = new List<(int Index, LegalSectionType Type, string? Number, string? Heading)>();

        foreach (var (pattern, type) in SectionPatterns)
        {
            foreach (Match match in pattern.Matches(text))
            {
                var number = match.Groups.Count > 1 && match.Groups[1].Success ? match.Groups[1].Value.Trim() : null;
                var heading = match.Groups.Count > 2 && match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

                // If number is the only capture and looks like a heading, use it as heading
                if (heading == null && number != null && !Regex.IsMatch(number, @"^[\d.]+$"))
                {
                    heading = number;
                    number = null;
                }

                boundaries.Add((match.Index, type, number, heading));
            }
        }

        // Sort by position in document
        boundaries.Sort((a, b) => a.Index.CompareTo(b.Index));

        // Remove overlapping boundaries (keep the first one at each position)
        var filtered = new List<(int Index, LegalSectionType Type, string? Number, string? Heading)>();
        var lastEnd = -1;
        foreach (var b in boundaries)
        {
            if (b.Index > lastEnd)
            {
                filtered.Add(b);
                lastEnd = b.Index + 1;
            }
        }

        if (filtered.Count == 0)
        {
            // No legal structure detected — treat as a single section
            sections.Add(new LegalSection(
                text,
                LegalSectionType.Unknown,
                null,
                null,
                GetPageNumber(0, pageMap),
                FindCrossReferences(text),
                false));
            return sections;
        }

        // Extract text before first boundary
        if (filtered[0].Index > 0)
        {
            var preambleText = text[..filtered[0].Index].Trim();
            if (preambleText.Length > 0)
            {
                sections.Add(new LegalSection(
                    preambleText,
                    LegalSectionType.Preamble,
                    null,
                    null,
                    GetPageNumber(0, pageMap),
                    FindCrossReferences(preambleText),
                    false));
            }
        }

        // Extract each section
        for (var i = 0; i < filtered.Count; i++)
        {
            var start = filtered[i].Index;
            var end = i + 1 < filtered.Count ? filtered[i + 1].Index : text.Length;
            var sectionText = text[start..end].Trim();

            if (sectionText.Length == 0) continue;

            var isAmendment = filtered[i].Type == LegalSectionType.Amendment ||
                              AmendmentContentRegex().IsMatch(sectionText);

            sections.Add(new LegalSection(
                sectionText,
                filtered[i].Type,
                filtered[i].Number,
                filtered[i].Heading,
                GetPageNumber(start, pageMap),
                FindCrossReferences(sectionText),
                isAmendment));
        }

        return sections;
    }

    private static List<LegalChunkResult> ChunkSection(LegalSection section, int chunkSize, int overlap)
    {
        var chunks = new List<LegalChunkResult>();
        var sentences = SentenceRegex().Split(section.Text)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        var current = new List<string>();
        var currentTokens = 0;

        foreach (var sentence in sentences)
        {
            var tokens = EstimateTokens(sentence);
            if (currentTokens + tokens > chunkSize && current.Count > 0)
            {
                chunks.Add(CreateChunkResult(current, currentTokens, section));

                var overlapWords = string.Join(" ", current).Split(' ')
                    .TakeLast(Math.Max(overlap, 0)).ToArray();
                current = overlapWords.Length == 0
                    ? new List<string>()
                    : [string.Join(" ", overlapWords)];
                currentTokens = overlapWords.Length;
            }

            current.Add(sentence);
            currentTokens += tokens;
        }

        if (current.Count > 0)
        {
            chunks.Add(CreateChunkResult(current, currentTokens, section));
        }

        return chunks;
    }

    private static LegalChunkResult CreateChunkResult(
        List<string> sentences, int tokenCount, LegalSection section)
    {
        return new LegalChunkResult(
            Text: string.Join(" ", sentences),
            TokenCount: tokenCount,
            SectionType: section.Type,
            SectionNumber: section.Number,
            SectionHeading: section.Heading,
            PageNumber: section.PageNumber,
            CrossReferences: section.CrossReferences,
            IsAmendment: section.IsAmendment);
    }

    private static string? FindCrossReferences(string text)
    {
        var matches = CrossRefRegex.Matches(text);
        if (matches.Count == 0) return null;
        var refs = matches.Select(m => m.Value.Trim()).Distinct().ToList();
        return string.Join("; ", refs);
    }

    private static List<(int Position, int Page)> BuildPageMap(string text, MatchCollection pageBreaks)
    {
        var map = new List<(int Position, int Page)> { (0, 1) };
        var page = 1;

        foreach (Match m in pageBreaks)
        {
            page++;
            map.Add((m.Index, page));
        }

        return map;
    }

    private static int? GetPageNumber(int position, List<(int Position, int Page)> pageMap)
    {
        if (pageMap.Count <= 1) return null;
        var page = 1;
        foreach (var (pos, p) in pageMap)
        {
            if (pos <= position) page = p;
            else break;
        }
        return page;
    }

    private static int EstimateTokens(string text)
        => Math.Max(1, text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

    // Generated regex patterns for legal section detection
    [GeneratedRegex(@"(?:^|\n)\s*ARTICLE\s+([IVXLCDM\d]+\.?)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ArticleRegex();

    [GeneratedRegex(@"(?:^|\n)\s*(?:SECTION|SEC\.?|§)\s+(\d+[\.\d]*)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex SectionRegex();

    [GeneratedRegex(@"(?:^|\n)\s*(?:CLAUSE)\s+(\d+[\.\d]*)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ClauseRegex();

    [GeneratedRegex(@"(?:^|\n)\s*\(([a-z\d]+)\)\s+(.*?)(?=\n)", RegexOptions.Multiline)]
    private static partial Regex SubClauseRegex();

    [GeneratedRegex(@"(?:^|\n)\s*WHEREAS[,;:]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex WhereasRegex();

    [GeneratedRegex(@"(?:^|\n)\s*RECITAL\s*(\d*)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex RecitalRegex();

    [GeneratedRegex(@"(?:^|\n)\s*(?:DEFINITIONS?|DEFINED\s+TERMS)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex DefinitionRegex();

    [GeneratedRegex(@"(?:^|\n)\s*SCHEDULE\s+([IVXLCDM\d]+\.?)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ScheduleRegex();

    [GeneratedRegex(@"(?:^|\n)\s*EXHIBIT\s+([A-Z\d]+\.?)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ExhibitRegex();

    [GeneratedRegex(@"(?:^|\n)\s*APPENDIX\s+([A-Z\d]+\.?)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex AppendixRegex();

    [GeneratedRegex(@"(?:^|\n)\s*AMENDMENT\s+(\d*)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex AmendmentRegex();

    [GeneratedRegex(@"(?:^|\n)\s*(?:PREAMBLE|INTRODUCTION|PRELIMINARY)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex PreambleRegex();

    [GeneratedRegex(@"(?:^|\n)\s*(?:IN\s+WITNESS\s+WHEREOF|SIGNATURES?|EXECUTED\s+BY)\s*[:\-–—.]?\s*(.*?)(?=\n)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex SignatureRegex();

    [GeneratedRegex(@"(?:(?:Section|Article|Clause|§)\s+[\d\.]+(?:\([a-z]\))?|(?:pursuant\s+to|in\s+accordance\s+with|as\s+defined\s+in|subject\s+to|referenced\s+in)\s+(?:Section|Article|Clause|§)\s+[\d\.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex CrossReferenceRegex();

    [GeneratedRegex(@"\f|(?:^|\n)\s*[-–—]+\s*Page\s+\d+\s*[-–—]*\s*(?:\n|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex PageBreakPattern();

    [GeneratedRegex(@"(?:amended|modified|replaced|deleted|inserted|substituted)\s+(?:by|pursuant|in\s+accordance)", RegexOptions.IgnoreCase)]
    private static partial Regex AmendmentContentRegex();

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SentenceRegex();

    private record LegalSection(
        string Text,
        LegalSectionType Type,
        string? Number,
        string? Heading,
        int? PageNumber,
        string? CrossReferences,
        bool IsAmendment);
}
