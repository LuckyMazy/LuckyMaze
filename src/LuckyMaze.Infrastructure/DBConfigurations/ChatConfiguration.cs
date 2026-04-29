using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasIndex(c => c.UserId);
            builder.Property(c => c.Title).HasMaxLength(200);

            builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Messages).WithOne(m => m.Chat).HasForeignKey(m => m.ChatId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
