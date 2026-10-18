using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Api.Data;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Encurtador.Tests.Integration;

public class ExpirationImmutabilityTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    [Fact]
    public void NoRegisteredEndpoint_UpdatesLinksOrExpiration()
    {
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>();

        foreach (var endpoint in endpoints)
        {
            var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [];

            Assert.DoesNotContain("PUT", methods);
            Assert.DoesNotContain("PATCH", methods);
            Assert.DoesNotContain("expir", endpoint.RoutePattern.RawText ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("PUT", "/api/links/ALTERAR22")]
    [InlineData("PATCH", "/api/links/ALTERAR22")]
    [InlineData("POST", "/api/links/ALTERAR22/expiration")]
    [InlineData("PUT", "/api/links/ALTERAR22/expiration")]
    public async Task ChangingExpirationOfExistingLink_IsRejectedAndKeepsOriginalDate(string method, string path)
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/fixo", "ALTERAR22", "2031-06-30"));

        var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = JsonContent.Create(new { expiresOn = "2040-01-01" }),
        };
        var response = await client.SendAsync(request);

        Assert.False(response.IsSuccessStatusCode);
        using var scope = factory.Services.CreateScope();
        var link = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Links
            .AsNoTracking().SingleAsync(link => link.Code == "ALTERAR22");
        Assert.Equal(new DateTime(2031, 6, 30, 23, 59, 59, DateTimeKind.Utc), link.ExpiresAtUtc);
    }
}
