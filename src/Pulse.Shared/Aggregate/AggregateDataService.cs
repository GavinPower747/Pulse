using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Pulse.Shared.Aggregate;

/// <summary>
/// Get data for a request and share the response across multiple components.
/// </summary>
/// <param name="cache"></param>
/// <param name="providers"></param>
/// <remarks>
/// This class is responsible for aggregating the requests together and
/// only fulfilling the request once. Caching or returning the inprogress task to
/// other UI components that are asking for the same piece of data.
///
/// This class should only live for as long as the request lives and no longer.
/// It's not designed to cache data across requests. Just to save on multiple DB trips
/// in the same page render.
/// </remarks>
public class AggregateDataService(
    MemoryCache cache,
    IDictionary<Type, AggregateDataProvider> providers,
    ILogger<AggregateDataService> logger
)
{
    private readonly MemoryCache _cache = cache;
    private readonly IDictionary<Type, AggregateDataProvider> _providers = providers;
    private readonly ConcurrentDictionary<string, object> _inProgressRequests = [];
    private readonly ILogger<AggregateDataService> _logger = logger;

    public Task<T?> EnlistDataRequest<T, TReq>(TReq request)
        where T : class
        where TReq : AggregateDataRequest<T>
    {
        if (_cache.TryGetValue(request.CacheKey, out var cached) && cached is T typedCached)
        {
            return Task.FromResult<T?>(typedCached);
        }

        var task =
            (Task<AggregateDataResponse<T?>>)
                _inProgressRequests.GetOrAdd(
                    request.CacheKey,
                    _ => ExecuteDataRequest<T, TReq>(request)
                );

        return task.ContinueWith(
            res =>
            {
                // Clean up the in-progress request now that it's complete (success or failure)
                _inProgressRequests.TryRemove(request.CacheKey, out _);

                // If the task faulted, propagate the exception
                if (res.IsFaulted)
                {
                    throw res.Exception!.GetBaseException();
                }

                // If the task was canceled, propagate the cancellation
                if (res.IsCanceled)
                {
                    throw new TaskCanceledException();
                }

                return res.Result?.Data;
            },
            TaskContinuationOptions.ExecuteSynchronously
        );
    }

    public Task<T[]?> EnlistDataRequest<T, TKey, TReq>(TReq request)
        where T : class
        where TReq : AggregateDataBatchRequest<T, TKey>
    {
        if (_cache.TryGetValue(request.CacheKey, out var cached) && cached is T[] typedCached)
        {
            return Task.FromResult<T[]?>(typedCached);
        }

        var task =
            (Task<AggregateDataResponse<T[]>>)
                _inProgressRequests.GetOrAdd(
                    request.CacheKey,
                    _ => ExecuteDataRequest<T[], TReq>(request)
                );

        //Add a task for each individual sub-request
        foreach (var key in request.Keys)
        {
            var itemCacheKey = request.GetItemCacheKey(key);
            var individualItemTask = task.ContinueWith(res =>
            {
                var overallResult = res.Result; //By virtue of this method being called this task should already be complete

                if (overallResult is not AggregateDataBatchResponse<T, TKey> typedResult)
                {
                    _logger.LogError(
                        "Batch response for {RequestType} was not of the expected type.",
                        request.GetType().FullName
                    );

                    return null;
                }

                var individualItem = typedResult.GetItemForKey(key);

                if (individualItem is not null)
                {
                    _cache.Set(itemCacheKey, individualItem);
                }

                return individualItem;
            });

            var added = _inProgressRequests.TryAdd(itemCacheKey, individualItemTask);

            if (added)
            {
                //When the individual item task completes, remove it from the in-progress requests
                individualItemTask.ContinueWith(_ =>
                    _inProgressRequests.TryRemove(itemCacheKey, out var _)
                );
            }
        }

        return task.ContinueWith(
            res =>
            {
                // Clean up the in-progress request now that it's complete (success or failure)
                _inProgressRequests.TryRemove(request.CacheKey, out _);

                // If the task faulted, propagate the exception
                if (res.IsFaulted)
                    throw res.Exception!.GetBaseException();

                // If the task was canceled, propagate the cancellation
                if (res.IsCanceled)
                    throw new TaskCanceledException();

                return res.Result?.Data;
            },
            TaskContinuationOptions.ExecuteSynchronously
        );
    }

    private async Task<AggregateDataResponse<T>?> ExecuteDataRequest<T, TReq>(TReq request)
        where T : class
        where TReq : AggregateDataRequest<T>
    {
        if (!_providers.TryGetValue(request.GetType(), out var provider))
        {
            _logger.LogError(
                "No provider found for request type {RequestType}.",
                request.GetType().FullName
            );

            return default;
        }

        if (provider is not AggregateDataProvider<TReq, T> typedProvider)
        {
            _logger.LogError(
                "Provider found for {RequestType} but was of the wrong type. This has been misconfigured!",
                request.GetType().FullName
            );

            return default;
        }

        var response = await typedProvider.GetData(request);
        var result = response?.Data;

        // Cache the result for other components in this request
        if (result is not null)
        {
            _cache.Set(request.CacheKey, result);
        }

        return response;
    }
}
