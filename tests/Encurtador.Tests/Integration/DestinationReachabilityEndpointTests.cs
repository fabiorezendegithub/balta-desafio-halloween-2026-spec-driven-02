using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Integration;

public class DestinationReachabilityEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    [Fact]
    public async Task PostLinks_WithReachableDestination_Returns201()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://acessivel.example/pagina"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostLinks_WithUnreachableDestination_Returns400ProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://inacessivel.example/pagina"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
