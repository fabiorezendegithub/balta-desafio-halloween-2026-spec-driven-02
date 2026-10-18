using System.Security.Cryptography;

namespace Encurtador.Api.Application;

public interface IShortCodeGenerator
{
    string Generate();
}

public sealed class ShortCodeGenerator : IShortCodeGenerator
{
    // A-Z sem I, L e O, mais 2 a 9: sem caracteres ambíguos (D-05, RF-02).
    public const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public const int CodeLength = 7;

    public string Generate()
    {
        return new string(RandomNumberGenerator.GetItems(Alphabet.AsSpan(), CodeLength));
    }
}
