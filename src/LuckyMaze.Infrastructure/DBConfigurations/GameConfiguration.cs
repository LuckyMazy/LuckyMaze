using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.State)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(g => g.WinningExit)
                .HasMaxLength(50);

            builder.HasOne(g => g.Maze)
                .WithMany()
                .HasForeignKey(g => g.MazeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
