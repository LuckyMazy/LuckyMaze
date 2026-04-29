using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.DBConfigurations
{
    public class ModelConfiguration : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.ModelId).HasMaxLength(200);
            builder.Property(m => m.Name).HasMaxLength(200);
            builder.Property(m => m.PromptPricePerMillion).HasPrecision(18, 8);
            builder.Property(m => m.CompletionPricePerMillion).HasPrecision(18, 8);

            builder.HasIndex(m => m.ModelId).IsUnique();
        }
    }
}
