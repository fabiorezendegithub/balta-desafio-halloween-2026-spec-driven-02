using Encurtador.Api.Application;

namespace Encurtador.Tests.Unit;

public class TargetUrlValidatorTests
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://example.com/caminho?q=1#secao")]
    [InlineData("http://example.com/")]
    public void IsValid_AcceptsHttpAndHttps(string url)
    {
        Assert.True(TargetUrlValidator.IsValid(url));
    }

    [Theory]
    [InlineData("ftp://example.com/arquivo")]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:pessoa@example.com")]
    [InlineData("example.com")]
    [InlineData("/caminho/relativo")]
    [InlineData("https://")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_RejectsInvalidUrls(string? url)
    {
        Assert.False(TargetUrlValidator.IsValid(url));
    }

    [Fact]
    public void IsValid_AcceptsUrlWithExactly2048Characters()
    {
        var url = BuildUrlWithLength(TargetUrlValidator.MaxLength);

        Assert.True(TargetUrlValidator.IsValid(url));
    }

    [Fact]
    public void IsValid_RejectsUrlWith2049Characters()
    {
        var url = BuildUrlWithLength(TargetUrlValidator.MaxLength + 1);

        Assert.False(TargetUrlValidator.IsValid(url));
    }

    private static string BuildUrlWithLength(int length)
    {
        const string prefix = "https://example.com/";
        return prefix + new string('a', length - prefix.Length);
    }
}
