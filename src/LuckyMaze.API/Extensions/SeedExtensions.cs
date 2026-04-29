using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Seeders;

namespace LuckyMaze.API.Extensions
{
    public static class SeedExtensions
    {
        public static async Task<WebApplication> ApplySeedsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LuckyMazeDbContext>();

            await AdminSettingsSeeder.SeedAsync(db);

            return app;
        }
    }
}
