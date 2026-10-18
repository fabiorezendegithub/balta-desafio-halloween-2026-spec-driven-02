using Encurtador.Api.Data;
using Encurtador.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Encurtador.Tests.Unit;

public class LinkModelTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void Code_HasUniqueIndex_ToEnforceRN01()
    {
        using var context = CreateContext();

        var entity = context.Model.FindEntityType(typeof(Link))!;
        var codeProperty = entity.FindProperty(nameof(Link.Code))!;

        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.Properties.Single() == codeProperty);
    }

    [Fact]
    public void ExpiresAtUtc_IsNullable_SoLinkWithoutExpirationIsRepresented()
    {
        using var context = CreateContext();

        var property = context.Model.FindEntityType(typeof(Link))!.FindProperty(nameof(Link.ExpiresAtUtc))!;

        Assert.True(property.IsNullable);
    }

    [Fact]
    public void TargetUrl_AllowsUpTo2048Characters()
    {
        using var context = CreateContext();

        var property = context.Model.FindEntityType(typeof(Link))!.FindProperty(nameof(Link.TargetUrl))!;

        Assert.Equal(2048, property.GetMaxLength());
    }
}
