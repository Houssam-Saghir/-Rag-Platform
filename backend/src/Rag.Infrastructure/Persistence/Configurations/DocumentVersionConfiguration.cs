using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rag.Domain.Entities;

namespace Rag.Infrastructure.Persistence.Configurations;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions");
        builder.Property(x => x.ChangeDescription).HasMaxLength(2000);
        builder.Property(x => x.FilePath).HasMaxLength(500);
        builder.Property(x => x.ContentHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
        builder.HasIndex(x => x.IsCurrent);
    }
}
