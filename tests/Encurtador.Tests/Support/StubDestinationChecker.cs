using Encurtador.Api.Application;

namespace Encurtador.Tests.Support;

public sealed class StubDestinationChecker(bool reachable) : IDestinationChecker
{
    public Task<bool> IsPubliclyReachableAsync(string url, CancellationToken cancellationToken = default) =>
        Task.FromResult(reachable);
}
