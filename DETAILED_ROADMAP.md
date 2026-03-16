# Advanced .NET Learning Roadmap - Detailed Guide

**Last Updated**: March 2026
**Target Audience**: Intermediate .NET developers wanting to master security and performance
**Prerequisites**: C# fundamentals, async/await basics, HTTP protocol knowledge

---

## 📋 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Phase 1: Performance Optimization](#phase-1-performance-optimization)
3. [Phase 2: Security Fundamentals](#phase-2-security-fundamentals)
4. [Integration: Full-Stack Example](#integration-full-stack-example)
5. [Learning Methodology](#learning-methodology)
6. [Progression Checklist](#progression-checklist)

---

## Architecture Overview

### Project Structure Philosophy

```
AdvancedDotNetAPI/
├── Security/                          ← PHASE 2: Confidentiality & Authenticity
│   ├── Authentication/                ← Who are you? (JWT tokens)
│   ├── Cryptography/                  ← Secret message protection (encryption)
│   └── Validation/                    ← Is this safe? (input sanitization)
├── Performance/                       ← PHASE 1: Speed & Efficiency
│   ├── Caching/                       ← Reduce work (remember results)
│   ├── Async/                         ← Non-blocking I/O (scale efficiently)
│   └── Profiling/                     ← Measure everything (data-driven)
├── Benchmarks/                        ← Tools to measure improvements
└── Controllers/                       ← Real API combining everything
```

### Learning Dependencies

```
START HERE:
├─ Performance/Async/AsyncPatterns.cs  (Understand async execution model)
│  └─ Required by: Everything else (async is fundamental)
│
├─ Performance/Caching/CachingStrategies.cs  (Reduce database load)
│  └─ Used by: SecurityAndPerformanceController (cache login results)
│
├─ Security/Validation/InputValidator.cs  (Prevent injection attacks)
│  └─ Required by: Every API endpoint (first line of defense)
│
├─ Security/Cryptography/PasswordHasher.cs  (Secure password storage)
│  └─ Used by: Login endpoints (never store plaintext passwords)
│
└─ Security/Authentication/JwtTokenService.cs  (Issue & validate tokens)
   └─ Used by: Protected endpoints (verify user identity)

INTEGRATION:
└─ Controllers/SecurityAndPerformanceController.cs
   └─ Combines: All of the above in a working API
```

---

## Phase 1: Performance Optimization

### Core Concept: Why Performance Matters

**The Problem**:
- Each millisecond of latency affects user experience
- Server resources are finite (CPU, memory, threads)
- Inefficient code wastes battery on mobile, wastes electricity on servers
- At scale (millions of requests), 1% improvement = huge cost savings

**The Solution**:
- Write code that does work efficiently
- Reduce unnecessary work (caching)
- Don't block threads waiting for I/O (async)
- Measure before optimizing (profiling & benchmarking)

---

### 1.1 Understanding Async/Await

**File**: [Performance/Async/AsyncPatterns.cs](Performance/Async/AsyncPatterns.cs)

#### What is Async?

Async doesn't make code faster. It allows one thread to handle many operations while waiting for I/O.

**Synchronous Model** (Blocking):
```
Thread 1: [Request 1] → Database query (thread blocked, waiting) → Response
         [Thread wasted for 100ms]

Thread 2: [Request 2] → Database query (thread blocked, waiting) → Response
         [Thread wasted for 100ms]

Need 1000 threads for 1000 concurrent requests
```

**Asynchronous Model** (Non-blocking):
```
Thread 1: [Request 1] → Database query (thread yields, not blocked)
         ↓
         [Request 2 starts while Request 1 waits]
         ↓
         [Request 1 returns from database]
         ↓
         [Response to Request 1]

One thread handles 1000 requests efficiently!
```

#### Key Pattern: async/await

```csharp
// WRONG: Blocks thread while waiting
public string FetchData(string url)
{
    var result = new HttpClient().GetStringAsync(url).Result;  // BLOCKS!
    return result;
}

// RIGHT: Thread can do other work while waiting
public async Task<string> FetchDataAsync(string url)
{
    var result = await new HttpClient().GetStringAsync(url);  // Yields thread
    return result;
}
```

**What happens at the `await` keyword:**
1. Check if operation is complete (no, waiting for HTTP response)
2. Register continuation (resume here when done)
3. **Return control to caller** (thread goes idle/handles other requests)
4. When HTTP response arrives, resume from `await`

#### Performance Benefit

```
Sync approach: 1000 requests × 100ms I/O time = need 1000 threads
Async approach: 1000 requests × 100ms I/O time = need 10 threads (thread pool)

Result:
- Fewer threads = less memory (each thread = ~1MB stack)
- Less context switching = faster CPU
- Can handle more concurrent requests
```

#### Common Pitfall: Sync-over-Async

```csharp
// DON'T DO THIS (causes deadlocks and wastes benefits)
public IActionResult GetUser(int id)
{
    var user = FetchUserAsync(id).Result;  // BLOCKS entire ASP.NET context!
    return Ok(user);
}

// DO THIS INSTEAD
public async Task<IActionResult> GetUser(int id)
{
    var user = await FetchUserAsync(id);   // Properly async
    return Ok(user);
}
```

#### Exercise 1: AsyncLockingCache

**Objective**: Prevent "thundering herd" - multiple requests computing same value

**Problem**: Without locking, 100 concurrent requests all fetch same user from database

```csharp
// BAD: All 100 threads fetch from database simultaneously
var cache = new InMemoryCache<int, User>();
for(int i = 0; i < 100; i++)
{
    var user = cache.GetOrCreate(1,
        () => FetchUserFromDatabaseAsync(1).Result,  // 100× database hit!
        TimeSpan.FromMinutes(5));
}

// GOOD: First thread fetches, others wait for result (AsyncLockingCache)
var cache = new AsyncLockingCache<int, User>();
var tasks = new List<Task<User>>();
for(int i = 0; i < 100; i++)
{
    tasks.Add(cache.GetOrCreateAsync(1,
        () => FetchUserFromDatabaseAsync(1),        // 1× database hit
        TimeSpan.FromMinutes(5)));
}
await Task.WhenAll(tasks);
```

---

### 1.2 Memory Management & Buffer Pooling

**File**: [Performance/Async/AsyncPatterns.cs](Performance/Async/AsyncPatterns.cs) - BufferPooling class

#### The GC Pressure Problem

Every `new` allocation is garbage that must be cleaned up:

```csharp
// WORST: Creates 1000 byte arrays, all garbage collected
for(int i = 0; i < 1000; i++)
{
    byte[] buffer = new byte[4096];  // 1000 allocations × 4KB = 4MB garbage
    ProcessData(buffer);
}

// GC Pause Time: 10-50ms when cleaning up!
```

#### Solution 1: ArrayPool<T> - Reuse Buffers

```csharp
// BETTER: Rent from pool, return when done
var pool = ArrayPool<byte>.Shared;
for(int i = 0; i < 1000; i++)
{
    byte[] buffer = pool.Rent(4096);  // Reuses previous allocations
    try { ProcessData(buffer); }
    finally { pool.Return(buffer); }  // Back to pool for reuse
}

// Result: Almost zero garbage!
```

**Benefits**:
- Zero (or minimal) allocations in hot paths
- No GC pauses
- ASP.NET Core uses this internally for all network buffers

#### Solution 2: Span<T> & stackalloc - Stack Allocation

```csharp
// BEST for small buffers: Stack allocated (instant + zero GC)
Span<byte> buffer = stackalloc byte[256];  // On stack, not heap!
ProcessData(buffer);
// Automatically cleaned up when out of scope

// Span<T> advantages:
// - Zero heap allocation
// - Can point to stack, heap, or unmanaged memory
// - No garbage collection needed
```

**When to use each:**

| Scenario | Solution | Why |
|----------|----------|-----|
| Small buffer (< 1KB), short-lived | `stackalloc` | Instant, zero GC |
| Medium buffer (1-100KB), frequent allocation | `ArrayPool<T>` | Reuse, minimal GC |
| Large buffer (> 100KB) | Direct `new` | Stack has 1MB limit |
| Reading from network | `ArrayPool<T>` | Built-in to ASP.NET Core |

#### Exercise 2: String Concatenation Benchmark

**Problem**: Strings are immutable - each `+` creates new string

```csharp
// WORST: Creates new string for each iteration
string result = "";
for(int i = 0; i < 1000; i++)
{
    result += i.ToString();  // 1000 allocations!
}
// Result: ~1MB garbage, multiple GC collections

// GOOD: StringBuilder accumulates efficiently
var sb = new StringBuilder();
for(int i = 0; i < 1000; i++)
{
    sb.Append(i);  // Appends to buffer, minimal allocations
}
string result = sb.ToString();  // Single allocation
// Result: 1 allocation, no GC

// BEST: String.Join when building from collection
string result = string.Join("", Enumerable.Range(0, 1000));
// Result: Optimized by CLR
```

**Benchmark Results** (measured with BenchmarkDotNet):
```
Concatenation:    ~500μs, 1000 allocations, 5.5MB
StringBuilder:    ~10μs, 1 allocation, 8KB
String.Join:      ~5μs, 1 allocation, 8KB
```

---

### 1.3 Caching Strategy

**File**: [Performance/Caching/CachingStrategies.cs](Performance/Caching/CachingStrategies.cs)

#### Cache-Aside Pattern

```csharp
public User GetUser(int userId)
{
    // Step 1: Check cache first
    if(cache.TryGetValue(userId, out var user))
        return user;  // Cache hit! Return immediately

    // Step 2: Cache miss - fetch from expensive source
    user = database.GetUser(userId);

    // Step 3: Store in cache for next time
    cache.Set(userId, user, TimeSpan.FromMinutes(5));
    return user;
}
```

**Benefits**:
- User doesn't wait for cache to populate (lazy loading)
- Simple to understand and implement
- Works with any data source

**Challenges**:
- Cache invalidation (when to remove stale data)
- Thundering herd (multiple requests computing same value)
- Cache penetration (requests for non-existent items)

#### Cache Invalidation Strategies

**Problem**: The "Two Hard Things in CS" quote applies here

```csharp
// STRATEGY 1: Time-based (TTL)
cache.Set(userId, user, TimeSpan.FromMinutes(5));
// After 5 min: data might be stale, but guaranteed cleanup

// STRATEGY 2: Event-based (invalidate on change)
public void UpdateUser(User user)
{
    database.SaveUser(user);
    cache.Remove(user.Id);  // Immediately invalidate
}
// Problem: Must remember to invalidate everywhere

// STRATEGY 3: Hybrid (TTL + event)
cache.Set(userId, user, TimeSpan.FromMinutes(5));
events.OnUserUpdated += (id) => cache.Remove(id);
// Best: Eventually consistent (time cleanup) + immediate invalidation
```

#### LRU (Least Recently Used) Eviction

When cache grows too large, evict least-used entries:

```csharp
public void EvictLRU(int maxEntries)
{
    // Remove least-recently-used items until below max
    var lruItems = cache
        .OrderBy(x => x.AccessCount)
        .Take(cache.Count - maxEntries);

    foreach(var item in lruItems)
        cache.Remove(item.Key);
}

// Example: Keep cache under 1000 items
cache.EvictLRU(1000);
```

#### Exercise 3: Distributed Cache Fallback

**Objective**: L1 (in-memory) → L2 (Redis) → L3 (database)

```csharp
public async Task<User> GetUserAsync(int userId)
{
    // L1: In-memory cache (fastest)
    if(memoryCache.Get(userId) is User user)
        return user;

    // L2: Distributed cache (medium)
    var redisValue = await redisCache.GetAsync(userId);
    if(redisValue != null)
    {
        user = JsonSerializer.Deserialize<User>(redisValue);
        memoryCache.Set(userId, user, TimeSpan.FromMinutes(1));  // Populate L1
        return user;
    }

    // L3: Database (slowest)
    user = await database.GetUserAsync(userId);
    await redisCache.SetAsync(userId, JsonSerializer.Serialize(user),
        TimeSpan.FromMinutes(5));
    memoryCache.Set(userId, user, TimeSpan.FromMinutes(1));
    return user;
}
```

---

### 1.4 Benchmarking with BenchmarkDotNet

**File**: [Benchmarks/PerformanceBenchmarks.cs](Benchmarks/PerformanceBenchmarks.cs)

#### Why Measure?

```
Without measurement: "String concatenation is probably slow"
With measurement: "String concatenation is 100× slower than StringBuilder"
(Big difference in optimization priority!)
```

#### Running Benchmarks

```bash
# Build in Release mode (important!)
dotnet build -c Release

# Run specific benchmark
dotnet run -c Release --project Benchmarks/PerformanceBenchmarks.cs

# Output: HTML report in BenchmarkDotNet.Artifacts/
```

#### Interpreting Results

```
Method               Mean        Median      StdDev      Allocated
Concatenation        523.4 μs    512.0 μs    45.3 μs     5.50 MB
StringBuilder        10.8 μs     8.5 μs      2.1 μs      8.50 KB
StringJoin           4.3 μs      3.8 μs      1.2 μs      2.10 KB

Interpretation:
- Concatenation: 523 microseconds, 5.5MB garbage
- StringBuilder: 10.8 microseconds, 8.5KB garbage
- StringJoin: Best - 4.3 microseconds, 2.1KB garbage

Performance ratio: Concatenation is 123× slower than StringJoin!
Memory ratio: Concatenation allocates 2,600× more!
```

#### Exercise 4: Profile Your Application

**Using dotTrace (JetBrains)**:
1. Attach to running application
2. Let it run for 30 seconds
3. Stop profiling
4. Look for "hot spots" (functions taking most time)
5. Prioritize optimization by impact

**Free Alternative - ETW Profiling**:
```bash
# Windows Performance Analyzer (free)
xperf -start session -on Proc_Thread+DISK_IO+HARD_FAULTS -stackwalk profile

# Run your app for 30 seconds...

xperf -stop session
wpa merged.etl  # Opens Windows Performance Analyzer
```

---

## Phase 2: Security Fundamentals

### Core Concept: The Defense-in-Depth Strategy

```
Attack Layers:
1. Network → HTTPS/TLS (Encrypt in transit)
2. Input → Validation (Reject bad input)
3. Authentication → Verify identity (Who are you?)
4. Authorization → Check permissions (Can you do this?)
5. Business Logic → Encryption (Protect at rest)
6. Output → Encoding (Prevent XSS)

Defense-in-Depth: If one layer fails, others protect you
```

---

### 2.1 Input Validation: First Line of Defense

**File**: [Security/Validation/InputValidator.cs](Security/Validation/InputValidator.cs)

#### The Injection Attack Problem

Untrusted input can be interpreted as code:

```csharp
// VULNERABLE: User input becomes SQL
string username = GetUserInput();  // Attacker enters: " OR "1"="1
string query = $"SELECT * FROM users WHERE username = '{username}'";
// Results in: SELECT * FROM users WHERE username = '' OR '1'='1'
// Returns ALL users! (authentication bypass)

// SAFE: Input validated first
if(!InputValidator.ValidateUsername(username))
    return BadRequest("Invalid username");
// Rejects: " OR "1"="1 (contains invalid characters)
```

#### Whitelist vs Blacklist

**Blacklist Approach** (BAD):
```csharp
// Try to block known-bad patterns
if(input.Contains(";") || input.Contains("DROP") || input.Contains("--"))
    return BadRequest();
// Problem: Attackers find new patterns you didn't block
```

**Whitelist Approach** (GOOD):
```csharp
// Only allow known-good characters
var pattern = new Regex(@"^[a-zA-Z0-9_-]{3,32}$");
if(!pattern.IsMatch(username))
    return BadRequest("Invalid username");
// Problem: Attacker must work within your constraints
```

#### Types of Injection Attacks

| Attack Type | Example | Prevention |
|------------|---------|-----------|
| **SQL Injection** | `" OR "1"="1"` | Parameterized queries |
| **XSS** | `<script>alert('hacked')</script>` | Input validation + output encoding |
| **Command Injection** | `; rm -rf /` | Whitelist validation |
| **LDAP Injection** | `*)(uid=*))(|(uid=*` | Escape special characters |
| **Path Traversal** | `../../../../etc/passwd` | Reject `..`, validate against allowed paths |

#### Exercise 5: Build Input Validator

**Create a form validator that catches 10 OWASP Top 10 attacks**:

```csharp
public class FormValidator
{
    public bool ValidateSearchInput(string query)
    {
        // 1. Length check (prevent DoS)
        if(string.IsNullOrWhiteSpace(query) || query.Length > 1000)
            return false;

        // 2. SQL injection patterns
        if(InputValidator.ContainsSuspiciousSqlKeywords(query))
            return false;

        // 3. XSS patterns
        if(InputValidator.ContainsSuspiciousHtmlPatterns(query))
            return false;

        // 4. Path traversal
        if(InputValidator.ContainsPathTraversalPatterns(query))
            return false;

        return true;
    }
}
```

---

### 2.2 Cryptography: Protecting Secrets

**File**: [Security/Cryptography/PasswordHasher.cs](Security/Cryptography/PasswordHasher.cs)

#### Password Hashing: Never Store Plaintext

**The Disaster Scenario**:
```
User stores password: "MyPassword123"
Database is breached
Hacker has password list → tries on other sites (credential reuse)
Result: All accounts compromised!
```

**The Solution**: Hash passwords so only user knows original

```csharp
// WRONG: Plaintext storage (if DB breached, all passwords exposed)
database.SaveUser(new { username = "alice", password = "MyPassword123" });

// WRONG: MD5 hashing (can be reversed)
string passwordHash = MD5.ComputeHash("MyPassword123");  // Only 10 billion MD5s!

// RIGHT: PBKDF2 hashing (expensive to reverse)
var hasher = new PasswordHasher();
string passwordHash = hasher.HashPassword("MyPassword123");
// Takes 100ms to hash (makes brute force expensive)
// Database stores hash, not password
```

#### How PBKDF2 Works

```csharp
// Simulate PBKDF2 (Key Derivation):
// Input: password = "MyPassword123"
// Output: 256-bit hash that's slow to compute

Iterations: 10,000
PBKDF2(password, salt, iterations=10000, length=32)
  ↓
Hash the password 10,000 times
  ↓
Takes ~100ms per password attempt
  ↓
Brute force 8-character password: 2^56 = 72 quadrillion possibilities
At 1 billion/second (modern GPU): 2 million years!
```

#### Verification: Constant-Time Comparison

```csharp
// WRONG: String comparison leaks timing info
if(passwordHash == inputHash)
    return "Success";
// Problem: Takes longer to compare if more characters match
// Attacker measures response time to guess hash byte-by-byte

// RIGHT: Constant-time comparison
if(CryptographicOperations.FixedTimeEquals(passwordHash, inputHash))
    return "Success";
// Takes same time whether hashes match or not
// Attacker gains no timing information
```

#### Encryption: When You Need to Decrypt Later

```csharp
// PASSWORDS: Hash (one-way)
var passwordHash = hasher.HashPassword("password");
// Can't decrypt, only verify

// API KEYS: Encrypt (two-way)
byte[] encryptedKey = hasher.EncryptSensitiveData(apiKey, encryptionKey);
// Can decrypt later if needed for API calls
// Protects key at rest but allows authorized access
```

#### Algorithm Comparison

| Algorithm | Speed | Security | Use Case |
|-----------|-------|----------|----------|
| **MD5** | ⚡ Fast | ❌ Broken | ❌ Never use |
| **SHA1** | ⚡ Fast | ⚠️ Weak | ❌ Never use for passwords |
| **SHA256** | ⚡ Fast | ✅ Good | ✅ Use if Argon2 unavailable |
| **bcrypt** | 🐌 Slow | ✅ Good | ✅ Older systems |
| **PBKDF2** | 🐌 Slow | ✅ Good | ✅ .NET standard (this project) |
| **Argon2** | 🐌 Slow | ✅ Best | ✅ New systems (needs NuGet pkg) |

**Recommendation**:
1. **Best**: Use Argon2 (add NuGet: `Isopoh.Cryptography.Argon2`)
2. **Good**: Use PBKDF2 (built-in, this project)
3. **Older**: Use bcrypt (PHP/Python compatibility)
4. **Never**: MD5, SHA1, unsalted hashes

#### Exercise 6: Password Reset Flow

**Objective**: Implement secure password reset

```csharp
public class PasswordResetService
{
    // Step 1: User requests password reset
    public async Task<string> GenerateResetTokenAsync(string email)
    {
        var user = await database.GetUserByEmailAsync(email);
        if(user == null)
            return null;  // Don't reveal if user exists

        // Generate cryptographically random token
        byte[] randomBytes = new byte[32];
        using(var rng = RandomNumberGenerator.Create())
            rng.GetBytes(randomBytes);
        string token = Convert.ToBase64String(randomBytes);

        // Hash token (don't store plaintext token in DB!)
        string tokenHash = hasher.HashPassword(token);

        // Store hash with expiration (15 min)
        user.ResetTokenHash = tokenHash;
        user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
        await database.SaveUserAsync(user);

        // Send token to user's email (not the hash!)
        await emailService.SendResetLinkAsync(email, token);

        return "Check your email";
    }

    // Step 2: User clicks link with token, submits new password
    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await database.GetUserByEmailAsync(email);
        if(user == null || DateTime.UtcNow > user.ResetTokenExpiry)
            return false;  // Token expired or invalid

        // Verify token against stored hash
        if(!hasher.VerifyPassword(token, user.ResetTokenHash))
            return false;  // Invalid token

        // Hash new password
        user.PasswordHash = hasher.HashPassword(newPassword);
        user.ResetTokenHash = null;  // Clear token (one-time use)
        await database.SaveUserAsync(user);

        return true;
    }
}
```

---

### 2.3 Authentication: Proving Identity

**File**: [Security/Authentication/JwtTokenService.cs](Security/Authentication/JwtTokenService.cs)

#### The JWT (JSON Web Token) Concept

JWT solves: "How do I know you are who you claim to be?"

```
User login → Server verifies password → Issues token
User accesses resource → Presents token → Server verifies token
No need to store session on server!
```

#### JWT Structure

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkFsaWNlIiwiaWF0IjoxNTE2MjM5MDIyfQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

         ↓ Split by dots ↓

[Header].[Payload].[Signature]
```

**Header** (Base64 encoded JSON):
```json
{
  "alg": "HS256",    // Signature algorithm
  "typ": "JWT"       // Type
}
```

**Payload** (Base64 encoded JSON with claims):
```json
{
  "sub": "alice",          // Subject (who)
  "name": "Alice Smith",   // Name claim
  "roles": ["admin"],      // Roles claim
  "iat": 1516239022,       // Issued at (when)
  "exp": 1516242622        // Expiration (when to throw away)
}
```

**Signature** (Cryptographic proof):
```
HMACSHA256(
  base64(header) + "." + base64(payload),
  secret_key
)
= Proves token wasn't tampered with
```

#### Token Validation Process

```csharp
public ClaimsPrincipal? ValidateToken(string token)
{
    var handler = new JwtSecurityTokenHandler();

    try
    {
        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            // 1. Signature verification (token wasn't modified)
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            // 2. Issuer check (token from correct server)
            ValidateIssuer = true,
            ValidIssuer = "AdvancedDotNetAPI",

            // 3. Audience check (token for correct service)
            ValidateAudience = true,
            ValidAudience = "AdvancedDotNetAPIClient",

            // 4. Expiration check (token not too old)
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,  // No leeway

            RequireExpirationTime = true
        }, out SecurityToken validatedToken);

        return principal;  // Token is valid!
    }
    catch(SecurityTokenException ex)
    {
        // Token validation failed (tampered, expired, wrong issuer, etc.)
        return null;
    }
}
```

#### Token Lifespan Decision

```csharp
// SHORT-LIVED ACCESS TOKEN (15-30 minutes)
var accessToken = GenerateToken(userId, expirationMinutes: 15);
// Pro: If stolen, can only be used briefly
// Con: Users must refresh frequently

// LONG-LIVED REFRESH TOKEN (7 days, HTTP-only cookie)
var refreshToken = GenerateRefreshToken(userId, expirationDays: 7);
// Pro: Users don't need to re-login weekly
// Con: If stolen, more time to abuse

// Flow:
// 1. Login → Issue both tokens
// 2. Access resource → Use accessToken
// 3. AccessToken expires → Use refreshToken to get new accessToken
// 4. RefreshToken expires → Force re-login
```

#### Exercise 7: Implement OAuth2 Integration

**Objective**: Allow users to login with Google/Microsoft

```csharp
public class OAuthTokenService
{
    // Step 1: Redirect to provider
    public string GetAuthorizationUrl()
    {
        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={clientId}&" +
            $"redirect_uri={redirectUri}&" +
            $"response_type=code&" +
            $"scope=email%20profile";
    }

    // Step 2: Exchange code for access token
    public async Task<GoogleUserInfo?> ExchangeCodeForTokenAsync(string code)
    {
        var response = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new StringContent(
                $"code={code}&client_id={clientId}&client_secret={clientSecret}"));

        var tokenResponse = await response.Content
            .ReadAsAsync<GoogleTokenResponse>();

        // Validate ID token (signed JWT from Google)
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(tokenResponse.IdToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://accounts.google.com" },
                ValidateAudience = true,
                ValidAudience = clientId,
                IssuerSigningKeys = await GetGooglePublicKeysAsync()
            });

        return new GoogleUserInfo
        {
            Email = principal.FindFirst("email")?.Value,
            Name = principal.FindFirst("name")?.Value
        };
    }
}
```

---

### 2.4 Authorization: Enforcing Permissions

**File**: [Controllers/SecurityAndPerformanceController.cs](Controllers/SecurityAndPerformanceController.cs) - CreateUser endpoint

#### Role-Based Access Control (RBAC)

```csharp
[HttpPost("admin/create-user")]
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
    // 1. Authenticate: Is this a valid token?
    var principal = jwtService.ValidateToken(request.Token);
    if(principal == null)
        return Unauthorized();

    // 2. Authorize: Does user have admin role?
    var isAdmin = principal.FindAll(ClaimTypes.Role)
        .Any(c => c.Value == "admin");

    if(!isAdmin)
        return Forbid("Admin role required");  // 403 Forbidden

    // 3. Validate input
    // (see Input Validation section)

    // 4. Execute
    return Created(...);
}
```

**Key Difference**:
- **Authentication**: Verify WHO you are (username/password)
- **Authorization**: Check WHAT you can do (admin/user/guest)

#### Claim-Based Authorization (Advanced)

```csharp
// Instead of just "admin" role, include detailed claims
var token = GenerateToken(userId, username, new[]
{
    // Role claim
    new Claim(ClaimTypes.Role, "admin"),

    // Permission claims (fine-grained)
    new Claim("permission", "users:create"),
    new Claim("permission", "users:delete"),
    new Claim("permission", "users:read"),

    // Department claim
    new Claim("department", "security")
});

// Later, check specific permission
bool canDeleteUsers = principal.FindAll("permission")
    .Any(c => c.Value == "users:delete");
```

---

## Integration: Full-Stack Example

**File**: [Controllers/SecurityAndPerformanceController.cs](Controllers/SecurityAndPerformanceController.cs)

### The Login Endpoint: All Layers

```csharp
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // LAYER 1: Input Validation (Defense-in-depth)
    if(!InputValidator.ValidateUsername(request.Username))
        return BadRequest();
    if(InputValidator.ContainsSuspiciousSqlKeywords(request.Username))
        return BadRequest();

    // LAYER 2: Performance (Check cache)
    if(_userCache.Get(request.Username) is UserDto cachedUser)
    {
        // Verify password (still do this, don't skip for cached users!)
        if(!_passwordHasher.VerifyPassword(request.Password, ...))
            return Unauthorized();

        // Proceed to token generation
    }

    // LAYER 3: Cryptography (Verify password safely)
    if(!_userDatabase.TryGetValue(request.Username, out var dbEntry))
        return Unauthorized();

    if(!_passwordHasher.VerifyPassword(request.Password, dbEntry.PasswordHash))
        return Unauthorized();  // Timing-safe comparison

    // LAYER 4: Authentication (Issue token)
    var token = _jwtService.GenerateToken(
        request.Username,
        request.Username,
        dbEntry.Roles);

    // LAYER 5: Performance (Cache for next request)
    _userCache.Set(request.Username, cachedUser, TimeSpan.FromMinutes(5));

    return Ok(new { token });
}
```

**Layers Working Together**:
1. **Input validation** prevents injection
2. **Caching** reduces repeated work
3. **Password verification** uses secure hashing
4. **Token generation** creates proof of identity
5. **Response** gives client access token

---

## Learning Methodology

### How to Study Effectively

#### Method 1: Read, Understand, Apply

```
Step 1: Read the code (5-10 min)
  - Don't try to understand everything
  - Look for overall structure

Step 2: Read the comments (10-15 min)
  - Comments explain "why", not "what"
  - Understand design decisions

Step 3: Run the code (5 min)
  - See it working
  - Modify one line and see what breaks

Step 4: Modify the code (30 min)
  - Change parameters
  - Add logging
  - See how changes affect behavior

Step 5: Write similar code (30+ min)
  - Implement your own version
  - See what was confusing
  - Build confidence
```

#### Method 2: Benchmark-Driven Learning

```
1. Make hypothesis: "StringBuilder is faster than concatenation"

2. Write benchmark:
   - Concatenation loop
   - StringBuilder loop
   - Run with BenchmarkDotNet

3. Analyze results: "StringBuilder is 100× faster!"

4. Understand why:
   - Read BenchmarkDotNet output
   - Check allocation counts
   - Look at generated IL code

5. Apply lesson: Use StringBuilder everywhere?
   - No, only in loops
   - Single string = just use literal
```

#### Method 3: Threat Modeling for Security

```
1. Identify assets: What are we protecting?
   - User passwords
   - Authentication tokens
   - User data
   - Server resources

2. Identify threats: Who might attack?
   - Attackers: Malicious external users
   - Insiders: Disgruntled employees
   - Accidental misuse: Users making mistakes

3. Identify risks: How could they attack?
   - SQL injection (input validation failure)
   - Brute force password (weak hashing)
   - Token theft (insecure transmission)

4. Design defenses: What prevents each attack?
   - Validate all inputs
   - Use strong hashing (PBKDF2+)
   - Transmit over HTTPS only

5. Test defenses: Can we break it?
   - Try to inject SQL
   - Try to crack hash
   - Try to steal token
```

---

## Progression Checklist

### Week 1-2: Performance Foundations ✓

- [ ] Read `AsyncPatterns.cs` completely
- [ ] Understand: sync blocking vs async non-blocking
- [ ] Exercise: Modify `GetOrCreateAsync` to use different timeouts
- [ ] Benchmark: Compare string concatenation vs StringBuilder
- [ ] Understand: Why stackalloc is fast (stack vs heap)
- [ ] Exercise: Write code using `ArrayPool<T>`
- [ ] Read entire `CachingStrategies.cs`
- [ ] Understand: Cache-aside, LRU, TTL concepts
- [ ] Exercise: Add cache eviction to InMemoryCache

**Assessment**: Can you explain async/await to a junior dev? ✓

### Week 3-4: Security Fundamentals ✓

- [ ] Read `InputValidator.cs` completely
- [ ] Understand: Whitelist vs blacklist validation
- [ ] Exercise: Add validation for phone numbers, URLs
- [ ] Read `PasswordHasher.cs` completely
- [ ] Understand: Why passwords are hashed (one-way)
- [ ] Understand: Salt, iterations, timing-safe comparison
- [ ] Exercise: Implement password strength meter
- [ ] Read `JwtTokenService.cs` completely
- [ ] Understand: JWT structure and validation steps
- [ ] Exercise: Decode JWT online (jwt.io) and understand payload

**Assessment**: Can you explain why plaintext passwords are dangerous? ✓

### Week 5: Integration ✓

- [ ] Study `SecurityAndPerformanceController.cs`
- [ ] Trace each endpoint through all security layers
- [ ] Run API locally and test each endpoint
- [ ] Modify: Add new endpoint combining 2+ concepts
- [ ] Exercise: Add rate limiting to prevent brute force
- [ ] Exercise: Add request logging without logging passwords
- [ ] Understand: How security and performance work together

**Assessment**: Can you identify security/performance issues in code? ✓

### Week 6+: Deep Dives

Choose one topic to master:

**Option A: Advanced Performance**
- [ ] Implement distributed cache (Redis)
- [ ] Profile real application with dotTrace
- [ ] Identify and fix 3 performance issues
- [ ] Write benchmarks proving improvements

**Option B: Advanced Security**
- [ ] Implement OAuth2 login
- [ ] Add HTTPS certificate pinning
- [ ] Implement refresh token rotation
- [ ] Add rate limiting and IP blocking

**Option C: Full-Stack Feature**
- [ ] Add user registration endpoint
  - Input validation
  - Password hashing
  - Email verification
  - Rate limiting
  - Logging (no PII)
- [ ] Add profile update endpoint
  - Authentication
  - Authorization
  - Input validation
  - Cache invalidation
  - Change notifications

**Option D: Production Readiness**
- [ ] Add comprehensive logging
- [ ] Add distributed tracing
- [ ] Add monitoring/alerting
- [ ] Create deployment pipeline
- [ ] Document API (Swagger/OpenAPI)
- [ ] Load test with k6 or Apache JMeter

---

## Resources by Topic

### Performance Books
- **"CLR via C#"** by Jeffrey Richter (Chapter 21: Async)
- **"C# Player's Guide"** by RB Whitaker (Performance sections)

### Security Books
- **"Cryptography and Network Security"** by William Stallings
- **"The Web Application Hacker's Handbook"** (ethical hacking)

### Official Microsoft Docs
- Async best practices: https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
- Security: https://docs.microsoft.com/en-us/dotnet/standard/security/
- Performance: https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/performance/

### Online Learning
- **OWASP Top 10**: https://owasp.org/www-project-top-ten/
- **Hash Cracking Time**: https://crackstation.net/ (see how fast hashes crack)
- **JWT Debugger**: https://jwt.io/ (understand token structure)
- **Regex Tester**: https://regex101.com/ (understand validation patterns)

### Tools
- **BenchmarkDotNet**: Measure code performance
- **dotTrace**: Profile .NET applications
- **Roslyn Analyzers**: Static code analysis for performance/security
- **OWASP ZAP**: Automatically test for security issues
- **SonarQube**: Code quality scanning

---

## Conclusion

### The Big Picture

You now have a complete foundation in:

1. **Performance**: Async, caching, memory optimization, benchmarking
2. **Security**: Input validation, cryptography, authentication, authorization
3. **Integration**: How these concepts work together in real APIs

### Next Steps

1. **Master**: Pick one area and go deep
2. **Build**: Create projects applying these concepts
3. **Measure**: Use benchmarks and profilers on real code
4. **Share**: Teach others what you learned

### Remember

- **Performance without Security** = Fast attack surface
- **Security without Performance** = Slow, unusable system
- **Both Together** = Robust, efficient applications

Good luck on your journey! 🚀

---

*Last Updated: March 2026*
*Questions? File an issue or contribute improvements to this learning guide!*
