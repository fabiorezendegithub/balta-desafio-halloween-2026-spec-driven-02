using Encurtador.Api.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Encurtador.Tests.Support;

public sealed class EncurtadorWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TempDatabase _database = new();

    public ControllableTimeProvider Clock { get; } =
        new(new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _database.ConnectionString);
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDestinationChecker, FakeDestinationChecker>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _database.Dispose();
        }
    }
}
