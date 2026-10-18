using Encurtador.Api.Domain;

namespace Encurtador.Api.Application;

public interface ILinkRepository
{
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    Task<Link?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);

    // Incremento atômico no banco (D-02): nunca lê o valor para reescrevê-lo na aplicação.
    Task IncrementAccessCountAsync(string code, CancellationToken cancellationToken = default);

    // Retorna false quando o código já existe no banco (índice único, D-04).
    Task<bool> TryAddAsync(Link link, CancellationToken cancellationToken = default);
}
