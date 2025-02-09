using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace MegaMonster.Services.Card.Infrastructure.Redis;

public class RedisService(IDistributedCache cache) : IRedisService
{
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var serializedValue = JsonConvert.SerializeObject(value);
        await cache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedData = await cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }
        return JsonConvert.DeserializeObject<T>(cachedData);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }
}