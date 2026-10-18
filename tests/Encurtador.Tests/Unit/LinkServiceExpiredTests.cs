using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceExpiredTests
{
    [Fact]
    public async Task Resolve_ExpiredLink_ReturnsGoneWithoutCountingAccess()
    {
        var repository = new InMemoryLinkRepository();
        await repository.TryAddAsync(new Link
        {
            Code = "EXPIRADO2",
            TargetUrl = "https://example.com",
            ExpiresAtUtc = new DateTime(2026, 10, 17, 23, 59, 59, DateTimeKind.Utc),
            AccessCount = 4,
        });
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero));
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), clock);

        var result = await service.ResolveAsync("EXPIRADO2");

        Assert.Equal(ErrorKind.Gone, result.Error!.Kind);
        Assert.Equal(4, repository.Links.Single().AccessCount);
    }
}
