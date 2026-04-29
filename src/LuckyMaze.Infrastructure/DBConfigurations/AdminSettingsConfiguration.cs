using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class AdminSettingsConfiguration : IEntityTypeConfiguration<AdminSettings>
    {
        public void Configure(EntityTypeBuilder<AdminSettings> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.MaxPricePerMillionTokens).HasPrecision(18, 6);
            builder.Property(a => a.CostMultiplier).HasPrecision(8, 4);
            builder.Property(a => a.CreditsPerUsd).HasPrecision(18, 4);

            builder.ToTable(t => t.HasCheckConstraint("CK_AdminSettings_Singleton", $"\"Id\" = '{AdminSettings.SingletonId}'"));
        }
    }
}
