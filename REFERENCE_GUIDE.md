# .NET Security & Performance - Quick Reference Guide

Fast lookup for common patterns and best practices.

---

## Performance Patterns

### Async/Await Pattern

```csharp
// ✅ GOOD: Async all the way
public async Task<User> GetUserAsync(int id)
{
    return await database.GetUserAsync(id);
}

// ❌ BAD: Sync-over-async (blocks threads!)
public User GetUser(int id)
{
    return database.GetUserAsync(id).Result;
}
```

### ValueTask vs Task

```csharp
// Use ValueTask for frequently-called methods that usually complete synchronously
public ValueTask<User> GetCachedUserAsync(int id)
{
    if(cache.TryGetValue(id, out var user))
        return new ValueTask<User>(user);  // No allocation
    return new ValueTask<User>(FetchAsync(id));  // Must allocate
}

// Use Task for everything else
public async Task<User> FetchAsync(int id)
{
    return await database.GetUserAsync(id);
}
```

### Buffer Pooling

```csharp
// High-throughput scenario: Use ArrayPool
var pool = ArrayPool<byte>.Shared;
byte[] buffer = pool.Rent(4096);
try
{
    int bytesRead = stream.Read(buffer, 0, buffer.Length);
}
finally
{
    pool.Return(buffer);  // Always return!
}

// Small, short-lived buffer: Use stackalloc
Span<byte> buffer = stackalloc byte[256];  // Zero allocation
ProcessData(buffer);  // Auto-cleanup when out of scope
```

### String Building

```csharp
// ❌ Loop concatenation (allocates per iteration)
string result = "";
foreach(var item in items) result += item;

// ✅ StringBuilder (single allocation)
var sb = new StringBuilder();
foreach(var item in items) sb.Append(item);
return sb.ToString();

// ✅✅ String.Join (built-in optimization)
return string.Join(", ", items);
```

### Caching Pattern

```csharp
public T GetOrCache<T>(string key, Func<T> factory, TimeSpan ttl)
{
    if(cache.Get(key) is T cached)
        return cached;

    var value = factory();
    cache.Set(key, value, ttl);
    return value;
}

// Async version with thundering herd prevention
public async Task<T> GetOrCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
{
    if(asyncCache.Get(key) is T cached)
        return cached;

    return await asyncCache.GetOrCreateAsync(key, factory, ttl);
}
```

### Benchmarking

```csharp
[MemoryDiagnoser]
public class MyBenchmark
{
    [Benchmark]
    public void Method1() { /* ... */ }

    [Benchmark]
    public void Method2() { /* ... */ }
}

// Run:
// dotnet run -c Release -- --benchmarks
```

---

## Security Patterns

### Input Validation

```csharp
// ✅ Whitelist approach (secure)
if(!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]{3,32}$"))
    return BadRequest("Invalid username");

// ❌ Blacklist approach (insecure)
if(username.Contains(";") || username.Contains("DROP"))
    return BadRequest();  // Attackers find new patterns!
```

### Password Hashing

```csharp
// ✅ Hash before storage
var hash = new PasswordHasher().HashPassword(password);
database.SaveUser(new { username, passwordHash = hash });

// ❌ Store plaintext
database.SaveUser(new { username, password });  // DISASTER!
```

### Verify Password (Timing-Safe)

```csharp
// ✅ Timing-safe comparison
var hasher = new PasswordHasher();
if(hasher.VerifyPassword(inputPassword, storedHash))
    return Ok("Success");
else
    return Unauthorized();

// ❌ String comparison (leaks timing info)
if(inputPassword == storedPassword)  // INSECURE!
    return Ok("Success");
```

### JWT Token Generation

```csharp
var service = new JwtTokenService("secret-key-32-chars-minimum");
var token = service.GenerateToken(
    userId: "alice",
    username: "alice",
    roles: new[] { "admin", "user" }
);
// Token expires in 15 minutes by default
```

### JWT Token Validation

```csharp
var service = new JwtTokenService("secret-key-32-chars-minimum");
var principal = service.ValidateToken(token);

if(principal == null)
    return Unauthorized("Invalid token");

var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
```

### Sensitive Data Encryption

```csharp
// Encrypt (for data that needs to be decrypted later)
byte[] encryptionKey = new byte[32];  // 256-bit key
var hasher = new PasswordHasher();
byte[] encrypted = hasher.EncryptSensitiveData(apiKey, encryptionKey);
database.SaveApiKey(encrypted);

// Decrypt (when needed)
byte[] decrypted = hasher.DecryptSensitiveData(encrypted, encryptionKey);
var apiKey = Encoding.UTF8.GetString(decrypted);
```

### Role-Based Authorization

```csharp
[HttpPost("admin/create-user")]
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
    // 1. Authenticate (is token valid?)
    var principal = jwtService.ValidateToken(token);
    if(principal == null)
        return Unauthorized();

    // 2. Authorize (does user have admin role?)
    var isAdmin = principal.FindAll(ClaimTypes.Role)
        .Any(c => c.Value == "admin");

    if(!isAdmin)
        return Forbid("Requires admin role");

    // 3. Proceed
    return Created(nameof(GetProfile), new { id = newUserId });
}
```

---

## Common Attacks & Prevention

| Attack | Example | Prevention |
|--------|---------|-----------|
| **SQL Injection** | `" OR "1"="1"` | Parameterized queries, input validation |
| **XSS** | `<script>alert('hacked')</script>` | Input validation, output encoding, CSP |
| **Path Traversal** | `../../../../etc/passwd` | Reject `..`, validate path within allowed folder |
| **Brute Force** | 1000 login attempts/sec | Rate limiting, strong hashing |
| **Token Theft** | Steal JWT from localStorage | HTTPS only, HTTP-only cookies, short expiration |
| **Command Injection** | `; rm -rf /` | Whitelist validation, avoid shell execution |
| **CSRF** | Forged request from other site | CSRF tokens, SameSite cookies |
| **Timing Attack** | Measure response time | Constant-time comparison (FixedTimeEquals) |

---

## Gotchas & Common Mistakes

### Async Mistakes

```csharp
// ❌ Sync-over-async
var result = asyncMethod().Result;
var result = asyncMethod().Wait();

// ✅ Proper async
var result = await asyncMethod();
```

### Caching Mistakes

```csharp
// ❌ Never cache sensitive data indefinitely
cache.Set(password, hash, TimeSpan.MaxValue);

// ✅ Use short TTL for sensitive data
cache.Set(sessionId, data, TimeSpan.FromMinutes(15));
```

### Hashing Mistakes

```csharp
// ❌ Never use fast algorithms for passwords
var hash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(password));

// ✅ Use slow algorithms designed for passwords
var hash = new PasswordHasher().HashPassword(password);
```

### Validation Mistakes

```csharp
// ❌ Trust user input
var query = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ Validate then parameterize
if(!IsValidUsername(userName))
    return BadRequest();
var query = "SELECT * FROM users WHERE name = @name";
```

### Token Mistakes

```csharp
// ❌ Long-lived tokens
var token = GenerateToken(expirationDays: 365);

// ✅ Short-lived access tokens + refresh tokens
var accessToken = GenerateToken(expirationMinutes: 15);
var refreshToken = GenerateToken(expirationDays: 7);
```

---

## Decision Trees

### Should I use Async?

```
Is operation I/O-bound? (HTTP, database, file, network)
├─ YES → Use async (thread can handle other requests)
└─ NO (CPU-bound computation)
   └─ Consider Task.Run if doing many CPU-bound operations
```

### Should I cache this?

```
Is data expensive to compute/fetch?
├─ YES → Cache it
└─ NO (already fast)
   └─ Caching overhead might not be worth it

Is data mutable (changes frequently)?
├─ NO (static) → Cache with long TTL
├─ YES (frequently changing)
│  └─ Cache with short TTL or invalidate on change
└─ MAYBE (changes unpredictably) → Cache with medium TTL
```

### What algorithm should I use?

```
For PASSWORDS:
├─ New project → Argon2
├─ .NET only → PBKDF2
└─ Compatibility needed → bcrypt

For ENCRYPTION:
├─ Need decryption → AES-256-GCM
└─ Don't need decryption → Hash (one-way)

For HASHING (data integrity check):
├─ Performance critical → SHA256
├─ Need speed + security → Blake2b
└─ Never MD5 or SHA1
```

---

## Performance Benchmarks (Reference)

Typical measurements on modern hardware:

| Operation | Time | Allocations | Notes |
|-----------|------|-------------|-------|
| String concatenation (1000x) | 500 μs | 1000× | Allocates per iteration |
| StringBuilder (1000x) | 10 μs | 1× | Accumulates efficiently |
| String.Join | 5 μs | 1× | Optimized by CLR |
| MD5 hash | 1 μs | - | Don't use for passwords |
| PBKDF2 hash | 100 ms | - | Secure for passwords |
| Array access | 10 ns | - | Fastest |
| Dictionary lookup | 50 ns | - | Fast |
| Database query | 10 ms | - | Very slow (I/O) |
| Network request | 100 ms | - | Extremely slow |

**Rule**: I/O (10-1000ms) >> CPU (0.001ms) >> Memory (0.05ms)

---

## Configuration Checklist

### Before Production

**Security**:
- [ ] HTTPS enforced (redirect HTTP → HTTPS)
- [ ] Security headers set (CSP, HSTS, X-Frame-Options)
- [ ] Secrets not in code (use configuration/vaults)
- [ ] All inputs validated
- [ ] Sensitive data never logged
- [ ] Authentication required for protected endpoints
- [ ] Rate limiting implemented
- [ ] CORS properly configured

**Performance**:
- [ ] Caching strategy documented
- [ ] Async used for I/O operations
- [ ] Database connection pooling enabled
- [ ] Indexes created on frequently queried columns
- [ ] N+1 queries identified and fixed
- [ ] Compression enabled (gzip)
- [ ] Profiling shows no hot spots

**Operations**:
- [ ] Logging implemented (but no PII)
- [ ] Monitoring/alerting configured
- [ ] Health checks added
- [ ] Graceful shutdown handling
- [ ] Documentation complete
- [ ] Error handling appropriate
- [ ] Load testing completed

---

## Tools Quick Reference

| Tool | Purpose | When to Use |
|------|---------|-----------|
| **BenchmarkDotNet** | Measure code performance | Before/after optimization |
| **dotTrace** | Profile application | Identify performance bottlenecks |
| **Roslyn Analyzers** | Static code analysis | Every commit (in CI/CD) |
| **OWASP ZAP** | Security testing | Before production |
| **k6** | Load testing | Before scaling |
| **Wireshark** | Network analysis | Debugging network issues |
| **PerfView** | ETW tracing | Diagnosing production issues |

---

## Learning Path Summary

```
Week 1: Async fundamentals
  └─ Can explain: "Why sync-over-async is bad"

Week 2: Memory optimization
  └─ Can explain: "Why string concatenation is slow"

Week 3: Caching
  └─ Can explain: "Cache-aside pattern"

Week 4: Input validation
  └─ Can explain: "Why whitelist > blacklist"

Week 5: Password security
  └─ Can explain: "Why passwords must be hashed"

Week 6: Authentication
  └─ Can explain: "How JWT tokens work"

Week 7: Integration
  └─ Can explain: "How security & performance work together"

Week 8+: Specialization
  └─ Can build: Production-ready systems with both
```

---

## More Resources

- JWT: https://jwt.io
- Regex: https://regex101.com
- Password hash strength: https://crackstation.net
- Security headers: https://securityheaders.com
- OWASP Top 10: https://owasp.org/www-project-top-ten/

---

*Last Updated: March 2026 | For questions, refer to DETAILED_ROADMAP.md*
