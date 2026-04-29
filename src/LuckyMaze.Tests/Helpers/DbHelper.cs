using Microsoft.EntityFrameworkCore;
using LuckyMaze.Domain;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Tests.Helpers;

public static class DbHelper
{
    public static LuckyMazeDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<LuckyMazeDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new LuckyMazeDbContext(options);
    }

    public static async Task<LuckyMazeDbContext> CreateSeededContextAsync(
        AdminSettings? settings = null,
        string? dbName = null)
    {
        var context = CreateContext(dbName);

        context.AdminSettings.Add(settings ?? new AdminSettings());
        await context.SaveChangesAsync();

        return context;
    }
}
