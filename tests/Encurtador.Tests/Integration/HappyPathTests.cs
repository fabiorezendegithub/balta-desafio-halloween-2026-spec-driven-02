using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Encurtador.Tests.Integration;

public class HappyPathTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    private static readonly WebApplicationFactoryClientOptions NoRedirectOptions = new() { AllowAutoRedirect = false };

    [Fact]
    public async Task CreateWithCustomCode_ThenAccessThreeTimes_StatsReportThreeAccesses()
    {
        var client = factory.CreateClient(NoRedirectOptions);

        var created = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/caminho-feliz", "FELIZ926"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<CreateLinkResponse>();
        Assert.Equal("FELIZ926", body!.Code);

        for (var i = 0; i < 3; i++)
        {
            var redirect = await client.GetAsync("/FELIZ926");
            Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
            Assert.Equal("https://example.com/caminho-feliz", redirect.Headers.Location?.ToString());
        }

        var stats = await client.GetAsync("/api/links/FELIZ926/stats");
        Assert.Equal(HttpStatusCode.OK, stats.StatusCode);
        var payload = await stats.Content.ReadFromJsonAsync<LinkStatsResponse>();
        Assert.Equal(3, payload!.AccessCount);
    }
}
