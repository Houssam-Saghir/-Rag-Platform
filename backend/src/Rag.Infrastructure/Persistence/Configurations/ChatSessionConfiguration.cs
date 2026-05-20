using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rag.Domain.Entities;

namespace Rag.Infrastructure.Persistence.Configurations;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.ToTable("ChatSessions");
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasOne(x => x.Document).WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.SetNull);
    }
}
