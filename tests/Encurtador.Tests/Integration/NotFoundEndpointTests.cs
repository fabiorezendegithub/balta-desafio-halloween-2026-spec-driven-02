using System.Net;
using System.Text.Json;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Encurtador.Tests.Integration;

public class NotFoundEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    private static readonly WebApplicationFactoryClientOptions NoRedirectOptions = new() { AllowAutoRedirect = false };

    [Fact]
    public async Task GetCode_WhenLinkDoesNotExist_Returns404WithMessageThatLinkDoesNotExist()
    {
        var client = factory.CreateClient(NoRedirectOptions);

        var response = await client.GetAsync("/NAOEXISTE1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("O link não existe.", problem.GetProperty("detail").GetString());
    }
}
