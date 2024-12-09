using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class AsyncQueue<T>
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    public int Count => _queue.Count;
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);


    // 將項目加入隊列
    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        _signal.Release();
    }

    // 異步地從隊列中取出項目
    public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
    {
        await _signal.WaitAsync(cancellationToken);
        _queue.TryDequeue(out var item);
        return item;
    }
    public void Clear()
    {
        while (_queue.TryDequeue(out _)) { }
        // 重置信號量
        while (_signal.CurrentCount > 0)
        {
            _signal.Wait();
        }
    }


}