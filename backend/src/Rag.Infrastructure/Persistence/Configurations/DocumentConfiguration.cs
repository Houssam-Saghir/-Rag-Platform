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

        // Law-specific metadata
        builder.Property(x => x.LegalDocumentType).HasDefaultValue(Domain.Enums.LegalDocumentType.Unknown);
        builder.Property(x => x.Jurisdiction).HasMaxLength(200);
        builder.Property(x => x.PracticeArea).HasMaxLength(200);
        builder.Property(x => x.CaseNumber).HasMaxLength(100);
        builder.Property(x => x.MatterNumber).HasMaxLength(100);
        builder.Property(x => x.Parties).HasMaxLength(2000);
        builder.HasIndex(x => x.LegalDocumentType);
        builder.HasIndex(x => x.Jurisdiction);

        // Versioning
        builder.Property(x => x.CurrentVersion).HasDefaultValue(1);
        builder.HasMany(x => x.Versions).WithOne(x => x.Document).HasForeignKey(x => x.DocumentId);
    }
}
