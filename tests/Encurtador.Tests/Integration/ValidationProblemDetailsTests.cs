using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Integration;

public class ValidationProblemDetailsTests(EncurtadorWebApplicationFactory factory) : IClassFixture<EncurtadorWebApplicationFactory>
{
    [Fact]
    public async Task PostLinks_WithInvalidUrl_Returns400ProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/links", new { targetUrl = "ftp://example.com" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Contains("URL", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task PostLinks_WithMalformedJson_Returns400ProblemDetails()
    {
        var client = factory.CreateClient();
        var content = new StringContent("{ \"targetUrl\": ", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/links", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task PostLinks_WithoutBody_Returns400ProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/links", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
