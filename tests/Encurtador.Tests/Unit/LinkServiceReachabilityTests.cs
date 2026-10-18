using Encurtador.Api.Application;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceReachabilityTests
{
    [Fact]
    public async Task Create_WithUnreachableDestination_ReturnsValidationErrorAndStoresNothing()
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(false), TimeProvider.System);

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com/fora"));

        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
        Assert.Empty(repository.Links);
    }

    [Fact]
    public async Task Create_WithUnreachableDestinationAndCustomCode_ReturnsValidationErrorAndStoresNothing()
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(false), TimeProvider.System);

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com/fora", "PROPRIO22"));

        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
        Assert.Empty(repository.Links);
    }
}
