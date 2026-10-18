using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceResolveTests
{
    [Fact]
    public async Task Resolve_WithExistingCode_ReturnsTargetUrl()
    {
        var repository = new InMemoryLinkRepository();
        await repository.TryAddAsync(new Link { Code = "ABCD2345", TargetUrl = "https://example.com/destino" });
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), TimeProvider.System);

        var result = await service.ResolveAsync("ABCD2345");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/destino", result.Value!.TargetUrl);
    }

    [Fact]
    public async Task Resolve_WithUnknownCode_ReturnsNotFound()
    {
        var service = new LinkService(new InMemoryLinkRepository(), new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), TimeProvider.System);

        var result = await service.ResolveAsync("NAOEXISTE");

        Assert.Equal(ErrorKind.NotFound, result.Error!.Kind);
        Assert.Equal("O link não existe.", result.Error.Message);
    }
}
