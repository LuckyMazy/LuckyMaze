using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class MazeConfiguration : IEntityTypeConfiguration<Maze>
    {
        public void Configure(EntityTypeBuilder<Maze> builder)
        {
            builder.HasKey(m => m.Id);

            // Stored as text (containing JSON strings)
            builder.Property(m => m.GridData)
                .HasColumnType("text");

            builder.Property(m => m.Exits)
                .HasColumnType("text");
        }
    }
}
