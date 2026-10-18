using Encurtador.Api.Application;

namespace Encurtador.Tests.Unit;

public class CustomCodeValidatorTests
{
    [Theory]
    [InlineData("ABCD")]
    [InlineData("ABCDEFGHIJKLMNOPQRST")]
    [InlineData("A1B2")]
    [InlineData("ABCD9999")]
    public void IsValid_AcceptsLengthsFrom4To20WithAllowedCharacters(string code)
    {
        Assert.True(CustomCodeValidator.IsValid(code));
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("A")]
    [InlineData("")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    public void IsValid_RejectsLengthsOutsideRange(string code)
    {
        Assert.False(CustomCodeValidator.IsValid(code));
    }

    [Theory]
    [InlineData("ABC0")]
    [InlineData("abcd")]
    [InlineData("ABC D")]
    [InlineData("ABC-D")]
    [InlineData("ABC_D")]
    [InlineData("ABC!")]
    public void IsValid_RejectsZeroLowercaseSpacesAndSpecialCharacters(string code)
    {
        Assert.False(CustomCodeValidator.IsValid(code));
    }

    [Fact]
    public void IsValid_RejectsNull()
    {
        Assert.False(CustomCodeValidator.IsValid(null));
    }
}
