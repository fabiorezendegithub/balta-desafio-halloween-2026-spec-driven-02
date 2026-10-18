using Encurtador.Api.Application;
using Encurtador.Api.Domain;

namespace Encurtador.Tests.Support;

// Simula a corrida em que a checagem de existência não vê o código, mas o índice único o recusa.
public sealed class RacingLinkRepository : InMemoryLinkRepository
{
    public override Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
