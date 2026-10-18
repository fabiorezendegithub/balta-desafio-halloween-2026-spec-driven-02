using Encurtador.Api.Application;

namespace Encurtador.Tests.Support;

// Destinos com "inacessivel" no endereço são tratados como fora do ar; os demais, como acessíveis.
public sealed class FakeDestinationChecker : IDestinationChecker
{
    public Task<bool> IsPubliclyReachableAsync(string url, CancellationToken cancellationToken = default) =>
        Task.FromResult(!url.Contains("inacessivel", StringComparison.OrdinalIgnoreCase));
}
