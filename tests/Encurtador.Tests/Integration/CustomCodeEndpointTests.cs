using System.Net;
using System.Net.Http.Json;
using Encurtador.Api.Contracts;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Integration;

public class CustomCodeEndpointTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    [Fact]
    public async Task PostLinks_WithCustomCode_Returns201WithThatCode()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/custom", "CODPROP1"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateLinkResponse>();
        Assert.Equal("CODPROP1", body!.Code);
    }

    [Fact]
    public async Task PostLinks_WithCodeAlreadyInUse_Returns409ProblemDetails()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/1", "REPETIDO1"));

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/2", "REPETIDO1"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("ABC0")]
    [InlineData("abcd")]
    [InlineData("ABC D")]
    public async Task PostLinks_WithInvalidCustomCode_Returns400ProblemDetails(string code)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new CreateLinkRequest("https://example.com/x", code));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
