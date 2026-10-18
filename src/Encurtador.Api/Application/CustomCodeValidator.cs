namespace Encurtador.Api.Application;

public static class CustomCodeValidator
{
    public const int MinLength = 4;

    public const int MaxLength = 20;

    private const string Allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

    public static bool IsValid(string? code)
    {
        if (code is null || code.Length < MinLength || code.Length > MaxLength)
        {
            return false;
        }

        return code.All(character => Allowed.Contains(character));
    }
}
