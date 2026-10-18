using Encurtador.Api.Application;

namespace Encurtador.Tests.Unit;

public class ShortCodeGeneratorTests
{
    private const string OfficialAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";

    [Fact]
    public void Generate_ReturnsCodeWithSevenCharacters()
    {
        var code = new ShortCodeGenerator().Generate();

        Assert.Equal(7, code.Length);
    }

    [Fact]
    public void Generate_UsesOnlyCharactersOfTheOfficialAlphabet()
    {
        var generator = new ShortCodeGenerator();

        for (var i = 0; i < 1000; i++)
        {
            Assert.All(generator.Generate(), character => Assert.Contains(character, OfficialAlphabet));
        }
    }

    [Theory]
    [InlineData('I')]
    [InlineData('L')]
    [InlineData('O')]
    [InlineData('0')]
    [InlineData('1')]
    public void Alphabet_ExcludesAmbiguousCharacters(char forbidden)
    {
        Assert.DoesNotContain(forbidden, ShortCodeGenerator.Alphabet);
    }

    [Fact]
    public void Generate_ProducesDifferentCodes()
    {
        var generator = new ShortCodeGenerator();

        var codes = Enumerable.Range(0, 1000).Select(_ => generator.Generate()).Distinct().Count();

        Assert.True(codes > 990, $"Esperado quase 1000 códigos distintos, obtido {codes}.");
    }
}
