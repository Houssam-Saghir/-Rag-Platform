using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rag.Domain.Entities;

namespace Rag.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.Property(x => x.Content).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.SourceChunkIdsJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.ChatSessionId, x.CreatedAt });
    }
}
