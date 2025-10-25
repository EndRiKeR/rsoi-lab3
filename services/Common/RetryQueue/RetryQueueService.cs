using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Common.RetryQueue;

public class RetryQueueService
{
    private readonly ConcurrentQueue<RetryRequest> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(0);

    public RetryQueueService() { }

    public void Enqueue(RetryRequest request)
    {
        _queue.Enqueue(request);
        _semaphore.Release();
    }

    public async Task<RetryRequest?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        return _queue.TryDequeue(out var request) ? request : null;
    }

    public bool TryDequeue(out RetryRequest? request)
    {
        return _queue.TryDequeue(out request);
    }

    public int GetQueueCount()
    {
        return _queue.Count;
    }

    public bool IsEmpty()
    {
        return _queue.IsEmpty;
    }
}