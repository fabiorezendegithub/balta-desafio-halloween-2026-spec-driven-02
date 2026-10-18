using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Api.Data;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Encurtador.Tests.Integration;

public class ExpiredLinkEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    private static readonly WebApplicationFactoryClientOptions NoRedirectOptions = new() { AllowAutoRedirect = false };

    [Fact]
    public async Task GetCode_AfterExpiration_Returns410WithoutRedirectAndKeepsAccessCount()
    {
        var client = factory.CreateClient(NoRedirectOptions);
        var created = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/vencido", "VENCIDO22", "2026-10-18"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        await client.GetAsync("/VENCIDO22");

        factory.Clock.Now = new DateTimeOffset(2026, 10, 19, 0, 0, 1, TimeSpan.Zero);
        var response = await client.GetAsync("/VENCIDO22");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        Assert.Null(response.Headers.Location);
        using var scope = factory.Services.CreateScope();
        var link = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Links
            .AsNoTracking().SingleAsync(link => link.Code == "VENCIDO22");
        Assert.Equal(1, link.AccessCount);
    }
}
