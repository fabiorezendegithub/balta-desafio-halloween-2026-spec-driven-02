using Encurtador.Api.Application;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceCreateTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_WithValidUrl_StoresLinkWithGeneratedCodeAndZeroAccesses()
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("ABCD2345"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com/longo"));

        Assert.True(result.IsSuccess);
        Assert.Equal("ABCD2345", result.Value!.Code);
        var stored = Assert.Single(repository.Links);
        Assert.Equal("ABCD2345", stored.Code);
        Assert.Equal("https://example.com/longo", stored.TargetUrl);
        Assert.Equal(Now.UtcDateTime, stored.CreatedAtUtc);
        Assert.Equal(0, stored.AccessCount);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("nao-e-url")]
    [InlineData(null)]
    public async Task Create_WithInvalidUrl_ReturnsValidationErrorAndStoresNothing(string? url)
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("ABCD2345"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand(url));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
        Assert.Empty(repository.Links);
    }
}
