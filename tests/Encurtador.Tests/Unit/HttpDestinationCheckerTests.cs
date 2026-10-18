using System.Net;
using Encurtador.Api.Http;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class HttpDestinationCheckerTests
{
    private static HttpDestinationChecker CreateChecker(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond,
        TimeSpan? timeout = null)
    {
        var client = new HttpClient(new StubHttpMessageHandler(respond))
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(HttpDestinationChecker.TimeoutSeconds),
        };

        return new HttpDestinationChecker(client);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task IsPubliclyReachable_TrueForSuccessStatus(HttpStatusCode status)
    {
        var checker = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(status)));

        Assert.True(await checker.IsPubliclyReachableAsync("https://example.com"));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task IsPubliclyReachable_FalseForNonSuccessStatus(HttpStatusCode status)
    {
        var checker = CreateChecker((_, _) => Task.FromResult(new HttpResponseMessage(status)));

        Assert.False(await checker.IsPubliclyReachableAsync("https://example.com"));
    }

    [Fact]
    public async Task IsPubliclyReachable_FalseWhenNetworkFails()
    {
        var checker = CreateChecker((_, _) => throw new HttpRequestException("falha de rede"));

        Assert.False(await checker.IsPubliclyReachableAsync("https://example.com"));
    }

    [Fact]
    public async Task IsPubliclyReachable_FalseWhenDestinationDoesNotAnswerWithinTimeout()
    {
        var checker = CreateChecker(
            async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK);
            },
            timeout: TimeSpan.FromMilliseconds(50));

        Assert.False(await checker.IsPubliclyReachableAsync("https://example.com"));
    }

    [Fact]
    public void Timeout_IsFiveSeconds()
    {
        Assert.Equal(5, HttpDestinationChecker.TimeoutSeconds);
    }
}
