using Encurtador.Api.Application;
using Encurtador.Tests.Support;

namespace Encurtador.Tests.Unit;

public class LinkServiceCodeFormatTests
{
    [Theory]
    [InlineData("ABC")]
    [InlineData("ABC0")]
    [InlineData("abcd")]
    [InlineData("ABC D")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    public async Task Create_WithInvalidCustomCode_ReturnsValidationErrorAndStoresNothing(string code)
    {
        var repository = new InMemoryLinkRepository();
        var service = new LinkService(repository, new StubShortCodeGenerator("GERADO22"), new StubDestinationChecker(true), new FixedTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.CreateAsync(new CreateLinkCommand("https://example.com", code));

        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
        Assert.Empty(repository.Links);
    }
}
