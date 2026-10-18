using Encurtador.Api.Application;

namespace Encurtador.Tests.Support;

public sealed class StubShortCodeGenerator(string code) : IShortCodeGenerator
{
    public string Generate() => code;
}
