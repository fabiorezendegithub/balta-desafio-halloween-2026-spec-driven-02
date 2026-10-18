using Encurtador.Api.Application;
using Encurtador.Api.Domain;

namespace Encurtador.Tests.Support;

public class InMemoryLinkRepository : ILinkRepository
{
    private readonly List<Link> _links = [];

    public IReadOnlyList<Link> Links => _links;

    public virtual Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_links.Any(link => link.Code == code));

    public Task<Link?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_links.FirstOrDefault(link => link.Code == code));

    public Task IncrementAccessCountAsync(string code, CancellationToken cancellationToken = default)
    {
        var link = _links.First(existing => existing.Code == code);
        link.AccessCount++;
        return Task.CompletedTask;
    }

    public virtual Task<bool> TryAddAsync(Link link, CancellationToken cancellationToken = default)
    {
        if (_links.Any(existing => existing.Code == link.Code))
        {
            return Task.FromResult(false);
        }

        _links.Add(link);
        return Task.FromResult(true);
    }
}
