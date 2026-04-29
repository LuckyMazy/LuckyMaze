using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.HasIndex(u => u.ExternalId).IsUnique();
            builder.Property(u => u.ExternalId).HasMaxLength(100);
            builder.Property(u => u.Email).HasMaxLength(255);
            builder.Property(u => u.DisplayName).HasMaxLength(100);
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            builder.OwnsOne(u => u.Preferences);
        }
    }
}
