using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Encurtador.Tests.Integration;

public class CaseSensitivityTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    private static readonly WebApplicationFactoryClientOptions NoRedirectOptions = new() { AllowAutoRedirect = false };

    [Fact]
    public async Task GetCode_WithLowercaseVersionOfExistingCode_Returns404()
    {
        var client = factory.CreateClient(NoRedirectOptions);
        var created = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/caixa", "CASOSENS"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var lowercase = await client.GetAsync("/casosens");
        var exact = await client.GetAsync("/CASOSENS");

        Assert.Equal(HttpStatusCode.NotFound, lowercase.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, exact.StatusCode);
    }
}
