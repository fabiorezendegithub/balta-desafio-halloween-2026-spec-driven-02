using Encurtador.Api.Application;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceCustomCodeTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 10, 30, 0, TimeSpan.Zero);

    private static LinkService CreateService(ILinkRepository repository) =>
        new(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

    [Fact]
    public async Task Create_WithFreeCustomCode_StoresLinkWithThatCode()
    {
        var repository = new InMemoryLinkRepository();

        var result = await CreateService(repository).CreateAsync(new CreateLinkCommand("https://example.com", "MEULINK"));

        Assert.True(result.IsSuccess);
        Assert.Equal("MEULINK", result.Value!.Code);
        Assert.Equal("MEULINK", Assert.Single(repository.Links).Code);
    }

    [Fact]
    public async Task Create_WithCodeAlreadyInUse_ReturnsConflictAndDoesNotStoreSecondLink()
    {
        var repository = new InMemoryLinkRepository();
        var service = CreateService(repository);
        await service.CreateAsync(new CreateLinkCommand("https://example.com/a", "MEULINK"));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com/b", "MEULINK"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Error!.Kind);
        Assert.Single(repository.Links);
    }

    [Fact]
    public async Task Create_WhenUniqueIndexRejectsRace_ReturnsConflict()
    {
        var repository = new RacingLinkRepository();
        var service = CreateService(repository);
        await service.CreateAsync(new CreateLinkCommand("https://example.com/a", "MEULINK"));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com/b", "MEULINK"));

        Assert.Equal(ErrorKind.Conflict, result.Error!.Kind);
    }
}
