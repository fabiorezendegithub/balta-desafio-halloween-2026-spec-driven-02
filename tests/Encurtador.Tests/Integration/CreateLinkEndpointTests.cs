using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Integration;

public class CreateLinkEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    [Fact]
    public async Task PostLinks_WithValidUrl_Returns201WithCodeAndShortUrl()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/longo"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateLinkResponse>();
        Assert.NotNull(body);
        Assert.Equal(7, body.Code.Length);
        Assert.EndsWith($"/{body.Code}", body.ShortUrl);
    }

    [Fact]
    public async Task PostLinks_WithInvalidUrl_Returns400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("ftp://example.com"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
