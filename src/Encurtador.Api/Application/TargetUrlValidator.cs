namespace Encurtador.Api.Application;

public static class TargetUrlValidator
{
    public const int MaxLength = 2048;

    public static bool IsValid(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > MaxLength)
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var isHttp = uri.Scheme is "http" or "https";

        return isHttp && !string.IsNullOrEmpty(uri.Host);
    }
}
