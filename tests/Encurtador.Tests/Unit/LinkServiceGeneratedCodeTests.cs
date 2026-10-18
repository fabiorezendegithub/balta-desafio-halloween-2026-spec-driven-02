using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceGeneratedCodeTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_WhenGeneratedCodeCollides_GeneratesAnotherCodeAndUsesIt()
    {
        var repository = new InMemoryLinkRepository();
        await repository.TryAddAsync(new Link { Code = "AAAA2222", TargetUrl = "https://example.com/antigo" });
        var generator = new SequenceShortCodeGenerator("AAAA2222", "BBBB3333");
        var service = new LinkService(repository, generator, new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com/novo"));

        Assert.True(result.IsSuccess);
        Assert.Equal("BBBB3333", result.Value!.Code);
        Assert.Contains(repository.Links, link => link.Code == "BBBB3333" && link.TargetUrl == "https://example.com/novo");
    }

    [Fact]
    public async Task Create_WhenUniqueIndexRejectsGeneratedCode_GeneratesAnotherCode()
    {
        var repository = new RacingLinkRepository();
        await repository.TryAddAsync(new Link { Code = "AAAA2222", TargetUrl = "https://example.com/antigo" });
        var generator = new SequenceShortCodeGenerator("AAAA2222", "BBBB3333");
        var service = new LinkService(repository, generator, new StubDestinationChecker(true), new FixedTimeProvider(Now));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com/novo"));

        Assert.Equal("BBBB3333", result.Value!.Code);
    }

    [Fact]
    public async Task Create_WhenEveryGeneratedCodeCollides_Fails()
    {
        var repository = new InMemoryLinkRepository();
        await repository.TryAddAsync(new Link { Code = "AAAA2222", TargetUrl = "https://example.com/antigo" });
        var service = new LinkService(repository, new StubShortCodeGenerator("AAAA2222"), new StubDestinationChecker(true), new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateLinkCommand("https://example.com/novo")));
    }
}
