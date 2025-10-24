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
        _semaphore.Release(); // Увеличиваем счетчик семафора
    }

    public async Task<RetryRequest?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        // Ждем, пока в очереди появится элемент
        await _semaphore.WaitAsync(cancellationToken);
        
        // Достаем элемент из очереди
        if (_queue.TryDequeue(out var request))
        {
            return request;
        }
        
        return null;
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