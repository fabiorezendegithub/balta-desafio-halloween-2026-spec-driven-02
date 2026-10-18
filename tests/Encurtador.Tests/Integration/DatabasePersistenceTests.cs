using Encurtador.Api.Data;
using Encurtador.Api.Domain;
using Encurtador.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Encurtador.Tests.Integration;

public class DatabasePersistenceTests
{
    private static AppDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Migrations_ApplyToDatabase_AndLinkSurvivesNewContext()
    {
        using var database = new TempDatabase();

        await using (var first = CreateContext(database.ConnectionString))
        {
            await first.Database.MigrateAsync();
            Assert.Empty(await first.Database.GetPendingMigrationsAsync());

            first.Links.Add(new Link
            {
                Code = "ABCD1234",
                TargetUrl = "https://example.com/",
                CreatedAtUtc = new DateTime(2026, 10, 18, 10, 0, 0, DateTimeKind.Utc),
            });
            await first.SaveChangesAsync();
        }

        await using var second = CreateContext(database.ConnectionString);
        var saved = await second.Links.SingleAsync(link => link.Code == "ABCD1234");

        Assert.Equal("https://example.com/", saved.TargetUrl);
        Assert.Null(saved.ExpiresAtUtc);
        Assert.Equal(0, saved.AccessCount);
    }
}
