using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasIndex(m => new { m.ChatId, m.SequenceNumber });
            builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
            builder.Property(m => m.Model).HasMaxLength(200);
            builder.Property(m => m.ToolCallId).HasMaxLength(100);
            builder.Property(m => m.FinishReason).HasMaxLength(50);
            builder.Property(m => m.ToolCalls).HasColumnType("text");
            builder.Property(m => m.TokenUsage).HasMaxLength(50);
        }
    }
}
