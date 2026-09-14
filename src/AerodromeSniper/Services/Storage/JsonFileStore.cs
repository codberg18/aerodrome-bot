using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AerodromeSniper.Services.Storage;

public class JsonFileStore : IJsonStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _baseDir;
    private readonly ILogger<JsonFileStore> _logger;

    public JsonFileStore(string baseDir, ILogger<JsonFileStore>? logger = null)
    {
        _baseDir = baseDir;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<JsonFileStore>.Instance;
        Directory.CreateDirectory(_baseDir);
    }

    public async Task<T?> LoadAsync<T>(string name, CancellationToken ct = default) where T : class
    {
        var path = Path.Combine(_baseDir, name);
        if (!System.IO.File.Exists(path)) return null;
        try
        {
            await using var stream = System.IO.File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, Options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read {File}, backing it up", name);
            System.IO.File.Move(path, path + ".corrupt", overwrite: true);
            return null;
        }
    }

    public async Task SaveAsync<T>(string name, T value, CancellationToken ct = default) where T : class
    {
        Directory.CreateDirectory(_baseDir);
        var path = Path.Combine(_baseDir, name);
        var temp = path + ".tmp";
        await using (var stream = System.IO.File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, value, Options, ct);
        }
        System.IO.File.Move(temp, path, overwrite: true);
    }
}
