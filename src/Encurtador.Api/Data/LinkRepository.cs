using Encurtador.Api.Application;
using Encurtador.Api.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Encurtador.Api.Data;

public sealed class LinkRepository(AppDbContext context) : ILinkRepository
{
    private const int SqliteUniqueConstraintErrorCode = 2067;

    public Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default) =>
        context.Links.AnyAsync(link => link.Code == code, cancellationToken);

    public Task<Link?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        context.Links.AsNoTracking().FirstOrDefaultAsync(link => link.Code == code, cancellationToken);

    public Task IncrementAccessCountAsync(string code, CancellationToken cancellationToken = default) =>
        context.Links
            .Where(link => link.Code == code)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(link => link.AccessCount, link => link.AccessCount + 1),
                cancellationToken);

    public async Task<bool> TryAddAsync(Link link, CancellationToken cancellationToken = default)
    {
        context.Links.Add(link);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            context.Entry(link).State = EntityState.Detached;
            return false;
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteUniqueConstraintErrorCode };
}
