using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceStatsTests
{
    [Fact]
    public async Task GetStats_ForExistingCode_ReturnsCodeAndTotalAccesses()
    {
        var repository = new InMemoryLinkRepository();
        await repository.TryAddAsync(new Link { Code = "STATS222", TargetUrl = "https://example.com", AccessCount = 7 });
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), TimeProvider.System);

        var result = await service.GetStatsAsync("STATS222");

        Assert.True(result.IsSuccess);
        Assert.Equal("STATS222", result.Value!.Code);
        Assert.Equal(7, result.Value.AccessCount);
    }

    [Fact]
    public async Task GetStats_ForUnknownCode_ReturnsNotFound()
    {
        var service = new LinkService(new InMemoryLinkRepository(), new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), TimeProvider.System);

        var result = await service.GetStatsAsync("NAOEXISTE1");

        Assert.Equal(ErrorKind.NotFound, result.Error!.Kind);
    }
}
