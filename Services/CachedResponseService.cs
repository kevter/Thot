using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using Thoth.Infrastructure;
using Thoth.Models;

namespace Thoth.Services
{
    public class CachedResponseService : ICachedResponseService
    {
        private readonly ICacheProvider cacheProvider;

        public CachedResponseService(ICacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<Response> GetCachedResponse(string cacheKey, Cache cache, Func<Task<Response>> func)
        {
            var response = cacheProvider.GetFromCache<Response>(cacheKey);
            if (response != null) return response;

            // Key not in cache, so get data.
            response = await func();

            // Set cache options.
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetSlidingExpiration(TimeSpan.FromSeconds(cache.TimeSpan != 0 ? cache.TimeSpan : 1));

            cacheProvider.SetCache(cacheKey, response, cacheEntryOptions);

            return response;
        }

        public async Task<Response> GetCachedResponse(string cacheKey, SemaphoreSlim semaphore, Func<Task<Response>> func)
        {
            var users = cacheProvider.GetFromCache<Response>(cacheKey);

            if (users != null) return users;
            try
            {
                await semaphore.WaitAsync();
                users = cacheProvider.GetFromCache<Response>(cacheKey); // Recheck to make sure it didn't populate before entering semaphore
                if (users != null)
                {
                    return users;
                }
                users = await func();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(10));

                cacheProvider.SetCache(cacheKey, users, cacheEntryOptions);
            }
            finally
            {
                semaphore.Release();
            }

            return users;
        }
    }
}