using Encurtador.Api.Application;

namespace Encurtador.Tests.Support;

public sealed class SequenceShortCodeGenerator(params string[] codes) : IShortCodeGenerator
{
    private int _next;

    public string Generate() => codes[_next++];
}
