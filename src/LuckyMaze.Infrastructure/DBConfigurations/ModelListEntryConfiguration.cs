using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class ModelListEntryConfiguration : IEntityTypeConfiguration<ModelListEntry>
    {
        public void Configure(EntityTypeBuilder<ModelListEntry> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasIndex(m => m.ModelId).IsUnique();
            builder.Property(m => m.ModelId).HasMaxLength(200);
            builder.Property(m => m.ListType).HasConversion<string>().HasMaxLength(20);
        }
    }
}
