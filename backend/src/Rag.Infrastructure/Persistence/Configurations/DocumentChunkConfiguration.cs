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
    }
}
