namespace AerodromeSniper.Services.Storage;

public interface IJsonStore
{
    Task<T?> LoadAsync<T>(string name, CancellationToken ct = default) where T : class;
    Task SaveAsync<T>(string name, T value, CancellationToken ct = default) where T : class;
}
