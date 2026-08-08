using System.Globalization;
using System.Text.Json;
using Dapr.Client;
using Shared.Domain.Services;

namespace Core.Infrastructure.Services;

internal sealed class DaprCacheService(DaprClient daprClient) : ICacheService
{
    private const string StoreName = "cachestore";

    private readonly DaprClient _daprClient = daprClient;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _daprClient.GetStateAsync<T?>(StoreName, key, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var metadata = new Dictionary<string, string>
        {
            { "ttlInSeconds", ttl.TotalSeconds.ToString(CultureInfo.InvariantCulture) }
        };

        return _daprClient.SaveStateAsync(StoreName, key, value, metadata: metadata, cancellationToken: cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return _daprClient.DeleteStateAsync(StoreName, key, cancellationToken: cancellationToken);
    }
}
