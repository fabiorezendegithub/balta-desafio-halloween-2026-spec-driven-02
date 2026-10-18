namespace Encurtador.Api.Application;

public interface IDestinationChecker
{
    Task<bool> IsPubliclyReachableAsync(string url, CancellationToken cancellationToken = default);
}
