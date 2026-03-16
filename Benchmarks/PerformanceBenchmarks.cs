using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;
using System.Text;

namespace AdvancedDotNetAPI.Benchmarks;

/// <summary>
/// PHASE 1: Performance - Benchmarking & Profiling
///
/// Learning Goals:
/// - Measure actual performance, not guess
/// - Use BenchmarkDotNet for reliable results
/// - Understand allocation vs execution time trade-offs
/// - Profile real applications with dotTrace
///
/// Rule: "Premature optimization is the root of all evil"
/// Counter-rule: "But lack of attention to performance is a bug"
///
/// Solution: MEASURE, don't guess!
/// </summary>

[MemoryDiagnoser]  // Track memory allocations
[ShortRunJob]  // Quick benchmarking
public class StringOperationsBenchmark
{
    private string _testString = "The quick brown fox jumps over the lazy dog";
    private readonly string _separator = " ";

    /// <summary>
    /// Benchmark 1: String concatenation (BAD for performance)
    /// Each operation allocates a new string.
    /// </summary>
    [Benchmark]
    public string ConcatenationApproach()
    {
        string result = "";
        var parts = _testString.Split(_separator);

        foreach (var part in parts)
        {
            result += part + "_";  // Allocates new string each time!
        }

        return result;
    }

    /// <summary>
    /// Benchmark 2: StringBuilder (GOOD for performance)
    /// Single allocation at the end.
    /// </summary>
    [Benchmark]
    public string StringBuilderApproach()
    {
        var sb = new StringBuilder();
        var parts = _testString.Split(_separator);

        foreach (var part in parts)
        {
            sb.Append(part).Append("_");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Benchmark 3: String.Join (BEST for this case)
    /// Uses optimal internal implementation.
    /// </summary>
    [Benchmark]
    public string StringJoinApproach()
    {
        var parts = _testString.Split(_separator);
        return string.Join("_", parts) + "_";
    }

    /// <summary>
    /// Benchmark 4: Span<T> (ADVANCED - zero allocation for simple cases)
    /// For really performance-critical code.
    /// </summary>
    [Benchmark]
    public int SpanApproach()
    {
        ReadOnlySpan<char> span = _testString.AsSpan();
        int count = 0;

        // Count spaces without allocating
        foreach (var ch in span)
        {
            if (ch == ' ')
                count++;
        }

        return count;
    }
}

/// <summary>
/// Benchmark: Hashing algorithms
/// Demonstrates cryptographic function performance
/// </summary>
[MemoryDiagnoser]
public class HashingBenchmark
{
    private readonly byte[] _data = Encoding.UTF8.GetBytes(
        "The quick brown fox jumps over the lazy dog");

    [Benchmark]
    public byte[] SHA256Hash()
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            return sha.ComputeHash(_data);
        }
    }

    [Benchmark]
    public byte[] MD5Hash()
    {
        // DON'T USE FOR SECURITY - shown for comparison only
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            return md5.ComputeHash(_data);
        }
    }

    [Benchmark]
    public byte[] SHA512Hash()
    {
        // Alternative to SHA256, slower but more secure margin
        using (var sha = System.Security.Cryptography.SHA512.Create())
        {
            return sha.ComputeHash(_data);
        }
    }
}

/// <summary>
/// Benchmark: Collection operations
/// Shows allocation and performance differences
/// </summary>
[MemoryDiagnoser]
public class CollectionBenchmark
{
    private readonly List<int> _list = Enumerable.Range(0, 1000).ToList();

    [Benchmark]
    public int ArrayIteration()
    {
        int sum = 0;
        var array = _list.ToArray();  // Allocation!
        foreach (var item in array)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int ListIteration()
    {
        int sum = 0;
        foreach (var item in _list)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int DirectEnumeration()
    {
        return _list.Sum();  // Uses LINQ
    }

    [Benchmark]
    public int UnsafeIteration()
    {
        int sum = 0;
        // Direct array access - fastest but unsafe
        var array = _list;
        for (int i = 0; i < array.Count; i++)
            sum += array[i];
        return sum;
    }
}

/// <summary>
/// Entry point to run benchmarks.
/// Run: dotnet run -c Release --project . -- --benchmarks
/// </summary>
public class BenchmarkHelper
{
    public static void RunBenchmarks()
    {
        var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<StringOperationsBenchmark>();
        // BenchmarkRunner automatically generates HTML reports
        // Check BenchmarkDotNet.Artifacts folder for detailed results
    }
}

/// <summary>
/// PROFILING TOOLS GUIDE
///
/// 1. **JetBrains dotTrace**
///    - Most comprehensive .NET profiler
///    - CPU profiling, memory profiling, concurrency profiling
///    - Timeline view shows thread activity over time
///
/// 2. **Parallel Stacks (Visual Studio)**
///    - Debug > Windows > Parallel Stacks
///    - View all thread call stacks simultaneously
///    - Identify deadlocks and contention
///
/// 3. **Windows Performance Analyzer (ETW)**
///    - Free from Windows Performance Toolkit
///    - Kernel-level profiling
///    - CPU, memory, I/O analysis
///    - Download: https://docs.microsoft.com/en-us/windows-hardware/test/wpt/
///
/// 4. **PerfView (Microsoft)**
///    - Free tool for ETW tracing
///    - Excellent for diagnosing production issues
///    - Download: https://github.com/Microsoft/perfview
///
/// PROFILING WORKFLOW:
/// 1. Identify slow area with profiler
/// 2. Generate benchmarks for that specific code
/// 3. Measure baseline with BenchmarkDotNet
/// 4. Try optimizations
/// 5. Measure improvement
/// 6. Confirm with profiler (10%+ improvement expected)
///
/// COMMON FINDINGS:
/// - String operations cause excessive allocation
/// - LINQ over small collections vs loops
/// - Lock contention in multi-threaded code
/// - Database round-trips (N+1 problem)
/// - Excessive garbage collection (GC pause time)
/// </summary>
internal class ProfilingGuide
{
    // Placeholder for profiling guidance
}
