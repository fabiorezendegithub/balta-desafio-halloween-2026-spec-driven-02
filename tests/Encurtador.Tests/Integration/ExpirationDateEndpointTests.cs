using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Api.Data;
using Encurtador.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Encurtador.Tests.Integration;

public class ExpirationDateEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    [Fact]
    public async Task PostLinks_WithExpirationDate_Returns201AndStoresLastSecondOfThatDay()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/validade", "VALIDADE1", "2030-01-15"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var link = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Links
            .AsNoTracking().SingleAsync(link => link.Code == "VALIDADE1");
        Assert.Equal(new DateTime(2030, 1, 15, 23, 59, 59, DateTimeKind.Utc), link.ExpiresAtUtc);
    }

    [Fact]
    public async Task PostLinks_WithExpirationDateInThePast_Returns400ProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/passado", ExpiresOn: "2000-01-01"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
