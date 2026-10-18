using Encurtador.Api.Application;

namespace Encurtador.Api.Endpoints;

public static class ErrorResults
{
    public static IResult From(ServiceError error)
    {
        var (statusCode, title) = error.Kind switch
        {
            ErrorKind.Validation => (StatusCodes.Status400BadRequest, "Dados inválidos"),
            ErrorKind.Conflict => (StatusCodes.Status409Conflict, "Código já em uso"),
            ErrorKind.NotFound => (StatusCodes.Status404NotFound, "Link não encontrado"),
            ErrorKind.Gone => (StatusCodes.Status410Gone, "Link expirado"),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error.Kind, null),
        };

        return Results.Problem(detail: error.Message, statusCode: statusCode, title: title);
    }
}
