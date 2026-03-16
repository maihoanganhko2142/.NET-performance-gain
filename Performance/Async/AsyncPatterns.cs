using System.Buffers;

namespace AdvancedDotNetAPI.Performance.Async;

/// <summary>
/// PHASE 1: Performance - Async Patterns & Best Practices
///
/// Learning Goals:
/// - Understand async/await execution model
/// - Avoid async pitfalls (deadlocks, sync-over-async)
/// - Use ValueTask for performance-critical paths
/// - Implement proper async disposal patterns
///
/// PERFORMANCE MYTH: async makes things faster
/// REALITY: async allows efficient I/O without thread pool starvation
/// One thread handles many I/O operations by yielding while waiting
/// </summary>

/// <summary>
/// Anti-pattern: Sync-over-async blocks threads unnecessarily.
/// DON'T do this:
/// ```csharp
/// var result = asyncMethod().Result;  // BLOCKS THREAD!
/// var result = asyncMethod().Wait();  // BLOCKS THREAD!
/// ```
///
/// This can cause:
/// - Thread pool starvation (all threads blocked)
/// - Deadlocks (if called from sync context)
/// - Reduced scalability
/// </summary>
public class AsyncBestPractices
{
    /// <summary>
    /// GOOD: Pure async all the way.
    /// Allows thread to handle other work while I/O is in flight.
    /// </summary>
    public async Task<string> FetchDataAsync(string url)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }

    /// <summary>
    /// Use ValueTask<T> instead of Task<T> for:
    /// - Methods that synchronously complete most of the time
    /// - Performance-critical paths (allocations matter)
    ///
    /// PERFORMANCE BENEFIT: Avoids Task allocation when result is immediate
    /// ValueTask: 0 allocations for synchronous path
    /// Task: At least 1 allocation (88 bytes) even if immediate
    /// </summary>
    public ValueTask<string> GetCachedDataAsync(string key, Func<Task<string>> factory)
    {
        // Simulated cache hit - no allocation with ValueTask
        if (MemoryCache.TryGetValue(key, out var cached))
        {
            return new ValueTask<string>(cached);
        }

        // Cache miss - must await
        return new ValueTask<string>(factory());
    }

    /// <summary>
    /// Properly handle async disposal.
    /// IAsyncDisposable is called when using 'await using'.
    /// </summary>
    public class AsyncStreamProcessor : IAsyncDisposable
    {
        private readonly HttpClient _client = new();
        private bool _disposed = false;

        public async Task ProcessStreamAsync(string url)
        {
            var stream = await _client.GetStreamAsync(url);
            // Process stream
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            // Async cleanup
            await Task.Delay(100);  // Simulate async cleanup
            _client.Dispose();
            _disposed = true;
        }
    }

    // Usage: await using var processor = new AsyncStreamProcessor();

    /// <summary>
    /// Batching strategy - combine multiple operations.
    /// PERFORMANCE: Reduce overhead of many small operations.
    /// EXAMPLE: Batch database inserts instead of one-by-one.
    /// </summary>
    public async Task<List<string>> ProcessBatchAsync(
        IAsyncEnumerable<string> items,
        int batchSize = 100)
    {
        var batch = new List<string>(batchSize);
        var results = new List<string>();

        await foreach (var item in items)
        {
            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                // Process entire batch at once
                var batchResults = await ProcessItemsBatchAsync(batch);
                results.AddRange(batchResults);
                batch.Clear();
            }
        }

        // Process remaining items
        if (batch.Count > 0)
        {
            var finalResults = await ProcessItemsBatchAsync(batch);
            results.AddRange(finalResults);
        }

        return results;
    }

    private async Task<List<string>> ProcessItemsBatchAsync(List<string> batch)
    {
        // Simulated batch processing
        await Task.Delay(10);
        return batch.Select(x => $"processed_{x}").ToList();
    }

    /// <summary>
    /// Parallel async operations with throttling.
    /// PERFORMANCE: Use limited concurrency to avoid overwhelming resources.
    /// </summary>
    public async Task ProcessParallelWithThrottlingAsync(
        IEnumerable<string> items,
        Func<string, Task> processor,
        int maxConcurrency = 10)
    {
        using (var semaphore = new SemaphoreSlim(maxConcurrency))
        {
            var tasks = items.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await processor(item);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}

/// <summary>
/// Memory optimization using object pooling with Span<T>.
/// PHASE 1: Performance - Memory & GC optimization
///
/// GOAL: Reduce allocations and GC pressure
/// </summary>
public class BufferPooling
{
    private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

    /// <summary>
    /// Use ArrayPool to reuse buffers instead of allocating new ones.
    /// BENEFIT: Reduces garbage collection pressure significantly.
    /// EXAMPLE: Network libraries (ASP.NET Core uses this internally)
    /// </summary>
    public async Task<string> ReadFromStreamAsync(Stream stream, int bufferSize = 4096)
    {
        byte[] buffer = _bufferPool.Rent(bufferSize);
        try
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        finally
        {
            _bufferPool.Return(buffer);
        }
    }

    /// <summary>
    /// Use Span<T> and stackalloc for stack-allocated buffers.
    /// PERFORMANCE: Zero heap allocation for small buffers.
    ///
    /// WARNING: Only for small, short-lived buffers (<=1MB typical)
    /// Stack is limited (~1MB on 32-bit, ~4MB on 64-bit)
    /// </summary>
    public void ProcessSmallData()
    {
        Span<byte> buffer = stackalloc byte[256];  // Stack allocated!

        // Use buffer
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)i;
        }

        // No allocation, no GC when buffer goes out of scope
    }

    /// <summary>
    /// String optimization: avoid repeated string allocations.
    /// </summary>
    public string NormalizePathBad(string path)
    {
        // BAD: Creates temporary string, then another
        return path.ToLower().Replace("\\", "/").Trim();
    }

    public string NormalizePathGood(string path)
    {
        // GOOD: Use spans and builder to minimize allocations
        ReadOnlySpan<char> input = path.AsSpan().TrimEnd();
        var sb = new System.Text.StringBuilder(input.Length);

        foreach (var ch in input)
        {
            if (ch == '\\')
                sb.Append('/');
            else
                sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString();
    }
}

// Simulated cache for example
internal static class MemoryCache
{
    private static readonly Dictionary<string, string> _cache = new();

    public static bool TryGetValue(string key, out string? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public static void Set(string key, string value)
    {
        _cache[key] = value;
    }
}
