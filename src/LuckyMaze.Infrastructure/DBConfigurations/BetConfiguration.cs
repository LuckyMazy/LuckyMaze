using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class BetConfiguration : IEntityTypeConfiguration<Bet>
    {
        public void Configure(EntityTypeBuilder<Bet> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.ExitName)
                .HasMaxLength(50);

            builder.Property(b => b.Amount)
                .HasPrecision(18, 2);

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(b => b.Game)
                .WithMany()
                .HasForeignKey(b => b.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Player)
                .WithMany()
                .HasForeignKey(b => b.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
