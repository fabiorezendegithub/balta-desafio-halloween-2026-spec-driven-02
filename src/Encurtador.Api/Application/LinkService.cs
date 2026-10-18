using Encurtador.Api.Domain;

namespace Encurtador.Api.Application;

public sealed record CreateLinkCommand(string? TargetUrl, string? CustomCode = null, string? ExpiresOn = null);

public sealed record CreatedLink(string Code);

public sealed record RedirectTarget(string TargetUrl);

public sealed record LinkStats(string Code, long AccessCount);

public sealed class LinkService(
    ILinkRepository repository,
    IShortCodeGenerator codeGenerator,
    IDestinationChecker destinationChecker,
    TimeProvider timeProvider)
{
    private const int MaxGenerationAttempts = 10;
    private const string UnreachableDestinationMessage =
        "A URL de destino não está acessível publicamente. Verifique o endereço e tente novamente.";
    private const string LinkNotFoundMessage = "O link não existe.";
    private const string LinkExpiredMessage = "O link expirou.";
    private const string InvalidUrlMessage = "A URL de destino é inválida. Use http ou https, com até 2048 caracteres.";
    private const string CodeInUseMessage = "Este código já está em uso. Escolha outro código.";
    private const string InvalidExpirationMessage =
        "A data de expiração deve estar no formato AAAA-MM-DD.";
    private const string PastExpirationMessage = "A data de expiração não pode estar no passado.";
    private const string InvalidCodeMessage =
        "O código deve ter de 4 a 20 caracteres, usando apenas letras maiúsculas de A a Z e números de 1 a 9.";

    public async Task<ServiceResult<CreatedLink>> CreateAsync(
        CreateLinkCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!TargetUrlValidator.IsValid(command.TargetUrl))
        {
            return ServiceResult<CreatedLink>.Failure(ErrorKind.Validation, InvalidUrlMessage);
        }

        DateTime? expiresAtUtc = null;
        if (command.ExpiresOn is not null)
        {
            if (!LinkExpiration.TryParseDate(command.ExpiresOn, out var expiresOn))
            {
                return ServiceResult<CreatedLink>.Failure(ErrorKind.Validation, InvalidExpirationMessage);
            }

            expiresAtUtc = LinkExpiration.ToLastValidInstantUtc(expiresOn);

            if (expiresAtUtc < timeProvider.GetUtcNow().UtcDateTime)
            {
                return ServiceResult<CreatedLink>.Failure(ErrorKind.Validation, PastExpirationMessage);
            }
        }

        if (command.CustomCode is not null)
        {
            return await CreateWithCustomCodeAsync(command.TargetUrl!, command.CustomCode, expiresAtUtc, cancellationToken);
        }

        if (!await destinationChecker.IsPubliclyReachableAsync(command.TargetUrl!, cancellationToken))
        {
            return ServiceResult<CreatedLink>.Failure(ErrorKind.Validation, UnreachableDestinationMessage);
        }

        return await CreateWithGeneratedCodeAsync(command.TargetUrl!, expiresAtUtc, cancellationToken);
    }

    private async Task<ServiceResult<CreatedLink>> CreateWithGeneratedCodeAsync(
        string targetUrl,
        DateTime? expiresAtUtc,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            var code = codeGenerator.Generate();

            if (await repository.ExistsAsync(code, cancellationToken))
            {
                continue;
            }

            if (await repository.TryAddAsync(NewLink(code, targetUrl, expiresAtUtc), cancellationToken))
            {
                return ServiceResult<CreatedLink>.Success(new CreatedLink(code));
            }
        }

        throw new InvalidOperationException(
            "Não foi possível gerar um código único após várias tentativas.");
    }

    private async Task<ServiceResult<CreatedLink>> CreateWithCustomCodeAsync(
        string targetUrl,
        string code,
        DateTime? expiresAtUtc,
        CancellationToken cancellationToken)
    {
        if (!CustomCodeValidator.IsValid(code))
        {
            return ServiceResult<CreatedLink>.Failure(ErrorKind.Validation, InvalidCodeMessage);
        }

        if (await repository.ExistsAsync(code, cancellationToken))
        {
            return ServiceResult<CreatedLink>.Failure(ErrorKind.Conflict, CodeInUseMessage);
        }

        if (!await destinationChecker.IsPubliclyReachableAsync(targetUrl, cancellationToken))
        {
            return ServiceResult<CreatedLink>.Failure(ErrorKind.Validation, UnreachableDestinationMessage);
        }

        var added = await repository.TryAddAsync(NewLink(code, targetUrl, expiresAtUtc), cancellationToken);

        return added
            ? ServiceResult<CreatedLink>.Success(new CreatedLink(code))
            : ServiceResult<CreatedLink>.Failure(ErrorKind.Conflict, CodeInUseMessage);
    }

    public async Task<ServiceResult<RedirectTarget>> ResolveAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var link = await repository.FindByCodeAsync(code, cancellationToken);

        if (link is null)
        {
            return ServiceResult<RedirectTarget>.Failure(ErrorKind.NotFound, LinkNotFoundMessage);
        }

        if (LinkExpiration.IsExpired(link.ExpiresAtUtc, timeProvider.GetUtcNow().UtcDateTime))
        {
            return ServiceResult<RedirectTarget>.Failure(ErrorKind.Gone, LinkExpiredMessage);
        }

        await repository.IncrementAccessCountAsync(code, cancellationToken);

        return ServiceResult<RedirectTarget>.Success(new RedirectTarget(link.TargetUrl));
    }

    public async Task<ServiceResult<LinkStats>> GetStatsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var link = await repository.FindByCodeAsync(code, cancellationToken);

        return link is null
            ? ServiceResult<LinkStats>.Failure(ErrorKind.NotFound, LinkNotFoundMessage)
            : ServiceResult<LinkStats>.Success(new LinkStats(link.Code, link.AccessCount));
    }

    private Link NewLink(string code, string targetUrl, DateTime? expiresAtUtc = null) => new()
    {
        Code = code,
        TargetUrl = targetUrl,
        CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
        ExpiresAtUtc = expiresAtUtc,
        AccessCount = 0,
    };
}
