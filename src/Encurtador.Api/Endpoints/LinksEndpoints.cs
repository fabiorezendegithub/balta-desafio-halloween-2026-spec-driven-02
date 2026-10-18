using Encurtador.Api.Contracts;
using Encurtador.Api.Application;

namespace Encurtador.Api.Endpoints;

public static class LinksEndpoints
{
    public static IEndpointRouteBuilder MapLinkEndpoints(this IEndpointRouteBuilder app)
    {
        var links = app.MapGroup("/api/links");

        links.MapPost("", CreateLink).WithName("CreateLink");

        links.MapGet("/{code}/stats", GetLinkStats).WithName("GetLinkStats");

        app.MapGet("/{code}", RedirectToTarget).WithName("RedirectLink");

        return app;
    }

    private static async Task<IResult> GetLinkStats(
        string code,
        LinkService linkService,
        CancellationToken cancellationToken)
    {
        var result = await linkService.GetStatsAsync(code, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new LinkStatsResponse(result.Value!.Code, result.Value.AccessCount))
            : ErrorResults.From(result.Error!);
    }

    private static async Task<IResult> RedirectToTarget(
        string code,
        LinkService linkService,
        CancellationToken cancellationToken)
    {
        var result = await linkService.ResolveAsync(code, cancellationToken);

        return result.IsSuccess
            ? Results.Redirect(result.Value!.TargetUrl)
            : ErrorResults.From(result.Error!);
    }

    private static async Task<IResult> CreateLink(
        CreateLinkRequest? request,
        LinkService linkService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await linkService.CreateAsync(new CreateLinkCommand(request?.TargetUrl, request?.Code, request?.ExpiresOn), cancellationToken);

        if (!result.IsSuccess)
        {
            return ErrorResults.From(result.Error!);
        }

        var code = result.Value!.Code;
        var shortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{code}";

        return Results.Created(shortUrl, new CreateLinkResponse(shortUrl, code));
    }
}
