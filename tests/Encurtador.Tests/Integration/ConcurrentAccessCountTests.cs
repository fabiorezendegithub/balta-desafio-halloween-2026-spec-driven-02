using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Api.Data;
using Encurtador.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Encurtador.Tests.Integration;

public class ConcurrentAccessCountTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    private static readonly WebApplicationFactoryClientOptions NoRedirectOptions = new() { AllowAutoRedirect = false };

    [Fact]
    public async Task ConcurrentAccesses_DoNotLoseAnyCount()
    {
        const int accesses = 100;
        var client = factory.CreateClient(NoRedirectOptions);
        var created = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/concorrente", "CONCORR1"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var responses = await Task.WhenAll(
            Enumerable.Range(0, accesses).Select(_ => client.GetAsync("/CONCORR1")));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Redirect, response.StatusCode));
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var link = await context.Links.AsNoTracking().SingleAsync(link => link.Code == "CONCORR1");
        Assert.Equal(accesses, link.AccessCount);
    }
}
