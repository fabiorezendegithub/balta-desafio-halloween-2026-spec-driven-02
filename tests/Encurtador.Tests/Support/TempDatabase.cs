namespace Encurtador.Tests.Support;

public sealed class TempDatabase : IDisposable
{
    public TempDatabase()
    {
        FilePath = Path.Combine(Path.GetTempPath(), $"encurtador-tests-{Guid.NewGuid():N}.db");
    }

    public string FilePath { get; }

    public string ConnectionString => $"Data Source={FilePath}";

    public void Dispose()
    {
        SqliteFileCleanup.Delete(FilePath);
    }
}

public static class SqliteFileCleanup
{
    public static void Delete(string filePath)
    {
        foreach (var path in new[] { filePath, filePath + "-shm", filePath + "-wal" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
