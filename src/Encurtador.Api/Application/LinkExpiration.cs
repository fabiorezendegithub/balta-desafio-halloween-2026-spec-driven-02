using System.Globalization;

namespace Encurtador.Api.Application;

public static class LinkExpiration
{
    private const string DateFormat = "yyyy-MM-dd";

    public static bool TryParseDate(string value, out DateOnly date) =>
        DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    // RN-03: o link expirado deixa de valer depois do último instante válido.
    public static bool IsExpired(DateTime? expiresAtUtc, DateTime nowUtc) =>
        expiresAtUtc is not null && nowUtc > expiresAtUtc.Value;

    // RF-06: a data vale até o último instante do dia em UTC, inclusive.
    public static DateTime ToLastValidInstantUtc(DateOnly date) =>
        date.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);
}
