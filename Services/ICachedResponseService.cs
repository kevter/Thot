using System;
using System.Threading;
using System.Threading.Tasks;
using Thoth.Models;

namespace Thoth.Services
{
    public interface ICachedResponseService
    {
        Task<Response> GetCachedResponse(string cacheKey, Cache cache, Func<Task<Response>> func);

        Task<Response> GetCachedResponse(string cacheKey, SemaphoreSlim semaphore, Func<Task<Response>> func);
    }
}