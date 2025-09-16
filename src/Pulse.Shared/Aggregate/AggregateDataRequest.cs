namespace Pulse.Shared.Aggregate;

// Open Generics in a collection would be nice.
public abstract class AggregateDataProvider(Type req, Type ret)
{
    public Type RequestType { get; init; } = req;
    public Type ResponseType { get; init; } = ret;
}

public abstract class AggregateDataProvider<TReq, TRet>()
    : AggregateDataProvider(typeof(TReq), typeof(TRet))
    where TReq : AggregateDataRequest<TRet>
{
    public abstract Task<AggregateDataResponse<TRet>> GetData(
        TReq request,
        CancellationToken ct = default
    );
}

public abstract class AggregateDataRequest<TRet>
{
    public abstract string CacheKey { get; }
}

public abstract class AggregateDataBatchRequest<TRet, TKey>(TKey[] keys)
    : AggregateDataRequest<TRet[]>
{
    public TKey[] Keys { get; init; } = keys;
    public abstract string GetItemCacheKey(TKey key);
}

public class AggregateDataResponse<TRet>(TRet data)
{
    public TRet Data { get; init; } = data;
}

public abstract class AggregateDataBatchResponse<TRet, TKey>(TRet[] data)
    : AggregateDataResponse<TRet[]>(data)
{
    public abstract TRet? GetItemForKey(TKey key);
}
