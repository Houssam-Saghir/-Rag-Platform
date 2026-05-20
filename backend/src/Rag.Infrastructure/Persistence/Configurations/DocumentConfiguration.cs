using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rag.Domain.Entities;

namespace Rag.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.FileType).IsRequired().HasMaxLength(20);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(x => x.UploadedByUserId);
        builder.HasIndex(x => x.Status);
    }
}
