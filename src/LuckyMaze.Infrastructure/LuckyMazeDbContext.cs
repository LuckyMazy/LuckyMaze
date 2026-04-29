using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure
{
    public class LuckyMazeDbContext(DbContextOptions<LuckyMazeDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LuckyMazeDbContext).Assembly);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplySaveChangesGuards();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplySaveChangesGuards();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplySaveChangesGuards()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    public class LuckyMazeDbContextFactory : IDesignTimeDbContextFactory<LuckyMazeDbContext>
    {
        public LuckyMazeDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LuckyMazeDbContext>();
            optionsBuilder.UseNpgsql();
            return new LuckyMazeDbContext(optionsBuilder.Options);
        }
    }
}
