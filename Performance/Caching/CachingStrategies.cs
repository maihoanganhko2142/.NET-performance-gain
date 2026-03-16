using System.Collections.Concurrent;

namespace AdvancedDotNetAPI.Performance.Caching;

/// <summary>
/// PHASE 1: Performance - Caching Strategies
///
/// Learning Goals:
/// - Understand cache layers (L1: memory, L2: distributed)
/// - Implement cache-aside pattern (lazy loading)
/// - Handle cache invalidation challenges
/// - Measure cache effectiveness
///
/// Cache Invalidation Problem:
/// "There are only two hard things in Computer Science:
///  cache invalidation and naming things." — Phil Karlton
/// </summary>

/// <summary>
/// L1: In-memory cache using ConcurrentDictionary.
/// PROS: Ultra-fast, no network latency
/// CONS: Single instance, not shared across processes, limited by RAM
/// USE CASE: Request-scoped caches, frequently accessed small datasets
/// </summary>
public class InMemoryCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, CacheEntry<TValue>> _cache;
    private readonly object _lockObject = new();

    public int Count => _cache.Count;

    public InMemoryCache()
    {
        _cache = new ConcurrentDictionary<TKey, CacheEntry<TValue>>();
    }

    /// <summary>
    /// Get value from cache, or return null if expired or missing.
    /// PERFORMANCE NOTE: O(1) lookup time
    /// </summary>
    public TValue? Get(TKey key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            // Check expiration
            if (entry.IsExpired())
            {
                _cache.TryRemove(key, out _);
                return default;
            }

            entry.AccessCount++;  // Track for LRU eviction
            return entry.Value;
        }

        return default;
    }

    /// <summary>
    /// Set value with absolute expiration.
    /// THREAD-SAFE: Uses ConcurrentDictionary for lock-free operation
    /// </summary>
    public void Set(TKey key, TValue value, TimeSpan expiration)
    {
        var entry = new CacheEntry<TValue>
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow.Add(expiration),
            CreatedAt = DateTime.UtcNow,
            AccessCount = 0
        };

        _cache[key] = entry;
    }

    /// <summary>
    /// Get-or-create pattern (cache-aside).
    /// If not in cache, execute factory function and cache result.
    /// </summary>
    public TValue GetOrCreate(TKey key, Func<TValue> factory, TimeSpan expiration)
    {
        if (Get(key) is TValue cached)
            return cached;

        lock (_lockObject)
        {
            // Double-check pattern to avoid thundering herd
            if (Get(key) is TValue cached2)
                return cached2;

            var value = factory();
            Set(key, value, expiration);
            return value;
        }
    }

    /// <summary>
    /// Async version of GetOrCreate.
    /// PERFORMANCE BEST PRACTICE for I/O-bound operations.
    /// </summary>
    public async Task<TValue> GetOrCreateAsync(
        TKey key,
        Func<Task<TValue>> factoryAsync,
        TimeSpan expiration)
    {
        if (Get(key) is TValue cached)
            return cached;

        lock (_lockObject)
        {
            if (Get(key) is TValue cached2)
                return cached2;

            // NOTE: Never await within a lock - this could deadlock
            // For async, use SemaphoreSlim or other async-aware lock
            var task = factoryAsync();
            return task.Result;  // WARNING: This blocks - see AsyncLock example
        }
    }

    /// <summary>
    /// Remove expired entries (background cleanup).
    /// Run periodically with a timer to free memory.
    /// </summary>
    public int RemoveExpiredEntries()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired())
            .Select(kvp => kvp.Key)
            .ToList();

        int removed = 0;
        foreach (var key in expiredKeys)
        {
            if (_cache.TryRemove(key, out _))
                removed++;
        }

        return removed;
    }

    /// <summary>
    /// Evict least recently used entries when cache is too large.
    /// MEMORY OPTIMIZATION: Prevent unbounded memory growth.
    /// </summary>
    public void EvictLRU(int maxEntries)
    {
        if (_cache.Count <= maxEntries)
            return;

        var lruEntries = _cache
            .OrderBy(kvp => kvp.Value.AccessCount)
            .Take(_cache.Count - maxEntries)
            .ToList();

        foreach (var entry in lruEntries)
        {
            _cache.TryRemove(entry.Key, out _);
        }
    }

    public void Clear() => _cache.Clear();
}

/// <summary>
/// Cache entry with expiration and access tracking.
/// </summary>
internal class CacheEntry<TValue>
{
    public TValue? Value { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AccessCount { get; set; }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}

/// <summary>
/// Async-safe cache locking using SemaphoreSlim.
/// PERFORMANCE NOTE: Prevents "thundering herd" - multiple threads
/// all computing the same expensive value when cache misses.
/// </summary>
public class AsyncLockingCache<TKey, TValue> where TKey : notnull
{
    private readonly InMemoryCache<TKey, TValue> _cache = new();
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = new();

    public TValue? Get(TKey key) => _cache.Get(key);

    public async Task<TValue> GetOrCreateAsync(
        TKey key,
        Func<Task<TValue>> factoryAsync,
        TimeSpan expiration)
    {
        // Return from cache if exists
        if (_cache.Get(key) is TValue cached)
            return cached;

        // Get or create lock for this key
        var lockSlim = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await lockSlim.WaitAsync();
        try
        {
            // Double-check: another task might have filled cache
            if (_cache.Get(key) is TValue cached2)
                return cached2;

            // Execute factory function
            var value = await factoryAsync();
            _cache.Set(key, value, expiration);
            return value;
        }
        finally
        {
            lockSlim.Release();

            // Cleanup lock if no longer needed
            if (_cache.Get(key) != null)
            {
                _locks.TryRemove(key, out _);
            }
        }
    }

    public void Clear()
    {
        _cache.Clear();
        _locks.Clear();
    }
}
