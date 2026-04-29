using Microsoft.EntityFrameworkCore;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.API.Extensions
{
    public static class MigrationExtensions
    {
        public static WebApplication ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LuckyMazeDbContext>();
            db.Database.Migrate();
            return app;
        }
    }
}
