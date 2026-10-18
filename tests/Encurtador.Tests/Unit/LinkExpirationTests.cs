using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkExpirationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public void ToLastValidInstantUtc_ReturnsLastSecondOfTheDayInUtc()
    {
        var date = new DateOnly(2026, 10, 25);

        var expiresAt = LinkExpiration.ToLastValidInstantUtc(date);

        Assert.Equal(new DateTime(2026, 10, 25, 23, 59, 59, DateTimeKind.Utc), expiresAt);
    }

    [Fact]
    public async Task Create_WithExpirationDate_StoresLastValidInstantOfThatDay()
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com", ExpiresOn: "2026-10-25"));

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2026, 10, 25, 23, 59, 59, DateTimeKind.Utc), Assert.Single(repository.Links).ExpiresAtUtc);
    }

    [Fact]
    public async Task Create_WithoutExpirationDate_StoresNullSoLinkNeverExpires()
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com"));

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(repository.Links).ExpiresAtUtc);
    }

    [Theory]
    [InlineData("25/10/2026")]
    [InlineData("2026-10-25T10:00:00Z")]
    [InlineData("2026-13-01")]
    [InlineData("")]
    public async Task Create_WithMalformedExpirationDate_ReturnsValidationError(string expiresOn)
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com", ExpiresOn: expiresOn));

        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
        Assert.Empty(repository.Links);
    }

    [Fact]
    public async Task Create_WithExpirationDateInThePast_ReturnsValidationErrorAndStoresNothing()
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com", ExpiresOn: "2026-10-17"));

        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
        Assert.Empty(repository.Links);
    }

    [Fact]
    public async Task Create_WithExpirationDateOfToday_IsAcceptedBecauseItIsValidUntilEndOfDay()
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com", ExpiresOn: "2026-10-18"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void IsExpired_AtLastValidInstant_IsFalse()
    {
        var expiresAt = new DateTime(2026, 10, 25, 23, 59, 59, DateTimeKind.Utc);

        Assert.False(LinkExpiration.IsExpired(expiresAt, expiresAt));
    }

    [Fact]
    public void IsExpired_OneTickAfterLastValidInstant_IsTrue()
    {
        var expiresAt = new DateTime(2026, 10, 25, 23, 59, 59, DateTimeKind.Utc);

        Assert.True(LinkExpiration.IsExpired(expiresAt, expiresAt.AddTicks(1)));
    }

    [Fact]
    public void IsExpired_WithoutExpirationDate_IsFalse()
    {
        Assert.False(LinkExpiration.IsExpired(null, new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
    }
}
