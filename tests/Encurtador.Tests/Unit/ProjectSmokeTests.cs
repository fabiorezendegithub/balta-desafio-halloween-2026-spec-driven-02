namespace Encurtador.Tests.Unit;

public class ProjectSmokeTests
{
    [Fact]
    public void ApiProject_IsReferenced()
    {
        Assert.Equal("Encurtador.Api", typeof(Program).Assembly.GetName().Name);
    }
}
