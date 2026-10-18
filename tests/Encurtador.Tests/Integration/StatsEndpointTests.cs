using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Encurtador.Tests.Integration;

public class StatsEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    private static readonly WebApplicationFactoryClientOptions NoRedirectOptions = new() { AllowAutoRedirect = false };

    [Fact]
    public async Task GetStats_WithoutAuthHeader_ReturnsTotalAccesses()
    {
        var client = factory.CreateClient(NoRedirectOptions);
        await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/estatisticas", "ESTATIS1"));
        await client.GetAsync("/ESTATIS1");
        await client.GetAsync("/ESTATIS1");

        var response = await client.GetAsync("/api/links/ESTATIS1/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stats = await response.Content.ReadFromJsonAsync<LinkStatsResponse>();
        Assert.Equal("ESTATIS1", stats!.Code);
        Assert.Equal(2, stats.AccessCount);
    }

    [Fact]
    public async Task GetStats_ForUnknownCode_Returns404ProblemDetails()
    {
        var client = factory.CreateClient(NoRedirectOptions);

        var response = await client.GetAsync("/api/links/NAOEXISTE1/stats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
