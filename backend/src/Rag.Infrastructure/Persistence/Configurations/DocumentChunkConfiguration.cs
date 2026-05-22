using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rag.Domain.Entities;

namespace Rag.Infrastructure.Persistence.Configurations;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");
        builder.Property(x => x.ChunkText).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.EmbeddingJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.DocumentId, x.ChunkIndex }).IsUnique();

        // Law-specific chunk metadata
        builder.Property(x => x.SectionType).HasDefaultValue(Domain.Enums.LegalSectionType.Unknown);
        builder.Property(x => x.SectionNumber).HasMaxLength(50);
        builder.Property(x => x.SectionHeading).HasMaxLength(500);
        builder.Property(x => x.CrossReferences).HasMaxLength(2000);
        builder.Property(x => x.VersionNumber).HasDefaultValue(1);
        builder.HasIndex(x => x.SectionType);
    }
}
