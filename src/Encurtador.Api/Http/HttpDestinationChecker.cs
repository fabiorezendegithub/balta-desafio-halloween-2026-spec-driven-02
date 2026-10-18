using Encurtador.Api.Application;

namespace Encurtador.Api.Http;

// Critério em D-06 e tempo limite em D-07 do plano.
public sealed class HttpDestinationChecker(HttpClient httpClient) : IDestinationChecker
{
    public const int TimeoutSeconds = 5;

    public async Task<bool> IsPubliclyReachableAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }
}
