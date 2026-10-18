using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Encurtador.Tests.Integration;

public class RedirectEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    private static readonly WebApplicationFactoryClientOptions NoRedirectOptions = new() { AllowAutoRedirect = false };

    [Fact]
    public async Task GetCode_ForExistingLink_Returns302ToTarget()
    {
        var client = factory.CreateClient(NoRedirectOptions);
        var created = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/destino-302", "REDIR392"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var response = await client.GetAsync("/REDIR392");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://example.com/destino-302", response.Headers.Location?.ToString());
    }
}
