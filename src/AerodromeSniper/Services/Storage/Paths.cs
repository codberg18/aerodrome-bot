using System.IO;

namespace AerodromeSniper.Services.Storage;

public static class Paths
{
    public static string DataDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AerodromeSniper");

    public static string LogsDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AerodromeSniper", "logs");

    public static string File(string name) => Path.Combine(DataDir, name);
}
