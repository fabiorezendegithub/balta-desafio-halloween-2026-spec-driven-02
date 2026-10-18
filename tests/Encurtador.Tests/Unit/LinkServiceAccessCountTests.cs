using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceAccessCountTests
{
    [Fact]
    public async Task Resolve_ValidCode_IncrementsAccessCountOnEachRequest()
    {
        var repository = new InMemoryLinkRepository();
        await repository.TryAddAsync(new Link { Code = "CONTAGEM2", TargetUrl = "https://example.com" });
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), TimeProvider.System);

        await service.ResolveAsync("CONTAGEM2");
        await service.ResolveAsync("CONTAGEM2");
        await service.ResolveAsync("CONTAGEM2");

        Assert.Equal(3, repository.Links.Single().AccessCount);
    }

    [Fact]
    public async Task Resolve_UnknownCode_DoesNotIncrementAnyLink()
    {
        var repository = new InMemoryLinkRepository();
        await repository.TryAddAsync(new Link { Code = "CONTAGEM2", TargetUrl = "https://example.com" });
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), TimeProvider.System);

        await service.ResolveAsync("NAOEXISTE1");

        Assert.Equal(0, repository.Links.Single().AccessCount);
    }
}
