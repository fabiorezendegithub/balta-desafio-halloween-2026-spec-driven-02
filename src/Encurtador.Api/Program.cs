using Encurtador.Api.Endpoints;
using Encurtador.Api.Errors;
using Encurtador.Api.Http;
using Encurtador.Api.Application;
using Encurtador.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<ILinkRepository, LinkRepository>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpClient<IDestinationChecker, HttpDestinationChecker>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(HttpDestinationChecker.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("EncurtadorLinks/1.0");
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.MapLinkEndpoints();

app.Run();

public partial class Program;
