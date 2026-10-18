namespace Encurtador.Api.Contracts;

public sealed record CreateLinkRequest(string? TargetUrl, string? Code = null, string? ExpiresOn = null);
