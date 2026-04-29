using Microsoft.EntityFrameworkCore;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.Seeders
{
    public static class AdminSettingsSeeder
    {
        public static async Task SeedAsync(LuckyMazeDbContext dbContext, CancellationToken cancellationToken = default)
        {
            if (await dbContext.AdminSettings.AnyAsync(cancellationToken))
                return;

            dbContext.AdminSettings.Add(new AdminSettings());
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
