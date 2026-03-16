# Quick Start Guide

## What You Have

This is a **full-stack .NET learning project** combining:
- ✅ **Phase 1 (Performance)**: Caching, async patterns, benchmarking
- ✅ **Phase 2 (Security)**: JWT, password hashing, input validation
- ✅ **Real API**: Controllers demonstrating all concepts together

## File Guide

```
Core Learning Modules
├── Security/Cryptography/PasswordHasher.cs      ← Password hashing, encryption
├── Security/Authentication/JwtTokenService.cs   ← JWT token generation/validation
├── Security/Validation/InputValidator.cs        ← Input sanitization
├── Performance/Caching/CachingStrategies.cs     ← In-memory caching patterns
├── Performance/Async/AsyncPatterns.cs           ← Async best practices
└── Benchmarks/PerformanceBenchmarks.cs          ← Benchmark examples

Real-World Example
└── Controllers/SecurityAndPerformanceController.cs  ← Full API combining everything

Documentation
├── LEARNING_GUIDE.md                            ← Week-by-week curriculum
└── QUICK_START.md                               ← This file
```

## Key Classes to Study

### Phase 1 (Performance)

**InMemoryCache<TKey, TValue>** [Caching/CachingStrategies.cs]
```csharp
var cache = new InMemoryCache<string, UserDto>();
cache.Set("user:123", user, TimeSpan.FromMinutes(5));
var cached = cache.Get("user:123");
```
- O(1) lookups
- Expiration handling
- LRU eviction
- Learn: Cache-aside pattern

**AsyncLockingCache<TKey, TValue>** [Caching/CachingStrategies.cs]
```csharp
var cache = new AsyncLockingCache<string, UserDto>();
var user = await cache.GetOrCreateAsync(
    userId,
    () => FetchFromDatabaseAsync(userId),
    TimeSpan.FromMinutes(5)
);
```
- Prevents thundering herd
- Thread-safe async operations
- Learn: Thundering herd problem

**BufferPooling & Span<T>** [Async/AsyncPatterns.cs]
```csharp
var buffer = ArrayPool<byte>.Shared.Rent(4096);
try { /* use buffer */ }
finally { ArrayPool<byte>.Shared.Return(buffer); }

Span<byte> stack = stackalloc byte[256];  // Zero allocation
```
- Zero allocations for small buffers
- GC pressure reduction
- Learn: Memory optimization

### Phase 2 (Security)

**JwtTokenService** [Authentication/JwtTokenService.cs]
```csharp
var service = new JwtTokenService("your-secret-key-32-chars-min");
var token = service.GenerateToken("alice", "alice", new[] { "admin" });
var principal = service.ValidateToken(token);
```
- Token generation with roles
- Expiration enforcement
- Signature verification
- Learn: Authentication & authorization

**PasswordHasher** [Cryptography/PasswordHasher.cs]
```csharp
var hasher = new PasswordHasher();
var hash = hasher.HashPassword("password123");
bool valid = hasher.VerifyPassword("password123", hash);
```
- PBKDF2 with HMAC-SHA256
- Random salt per password
- Timing-safe comparison
- Learn: Cryptographic security

**InputValidator** [Validation/InputValidator.cs]
```csharp
bool isEmail = InputValidator.ValidateEmail(email);
bool containsInjection = InputValidator.ContainsSuspiciousSqlKeywords(input);
var result = InputValidator.ValidateUserInput(input);
```
- Whitelist-based validation
- SQL injection prevention
- XSS attack detection
- Path traversal prevention
- Learn: Defense-in-depth

### Real API Example

**SecurityAndPerformanceController** [Controllers/SecurityAndPerformanceController.cs]
- `POST /api/v1/securityandperformance/login` - Authentication with password hashing
- `GET /api/v1/securityandperformance/profile` - Protected endpoint with JWT validation + caching
- `POST /api/v1/securityandperformance/admin/create-user` - Role-based authorization
- `GET /api/v1/securityandperformance/search` - Input validation example

## Running the Project

### Build
```bash
dotnet build AdvancedDotNetAPI.csproj
```

### Run API
```bash
dotnet run
```

### Run Benchmarks
```bash
dotnet run -c Release
# Then look for BenchmarkDotNet.Artifacts folder for results
```

### Test API Endpoints

```bash
# 1. Login
curl -X POST https://localhost:5001/api/v1/securityandperformance/login \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"alice_password_123"}'

# Response:
# {"token":"eyJhbGciOiJIUzI1NiIs..."}

# 2. Use token to access protected endpoint
TOKEN="eyJhbGciOiJIUzI1NiIs..."
curl https://localhost:5001/api/v1/securityandperformance/profile \
  -H "Authorization: Bearer $TOKEN"

# 3. Admin operation (admin account required)
curl -X POST https://localhost:5001/api/v1/securityandperformance/admin/create-user \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"username":"charlie","password":"password123","roles":["user"]}'
```

## Study Path

### Week 1-2: Performance Foundations
1. Read: `Performance/Async/AsyncPatterns.cs` - Buffer pooling, Span<T>
2. Code: Modify `AsyncPatterns.cs` to use different buffer sizes and measure allocations
3. Benchmark: `Performance/Caching/CachingStrategies.cs` vs. naive dictionary
4. Exercise: Implement a simple cache with TTL

### Week 3-4: Authentication & Cryptography
1. Read: `Security/Authentication/JwtTokenService.cs`
2. Code: Generate tokens, validate them, check expiration
3. Read: `Security/Cryptography/PasswordHasher.cs`
4. Exercise: Implement password reset with secure token

### Week 5: Full Integration
1. Study: `SecurityAndPerformanceController.cs` - See all concepts together
2. Test: Hit each endpoint and trace the security/performance layers
3. Modify: Add new endpoints with your own security/performance concerns
4. Benchmark: Measure response times with/without caching

## Common Questions

**Q: Why PBKDF2 instead of Argon2?**
A: .NET doesn't have native Argon2. Use NuGet package `Isopoh.Cryptography.Argon2` for production.

**Q: Should I use async everywhere?**
A: Only for I/O-bound operations (database, HTTP, file). Keep CPU-bound code sync.

**Q: How do I add Redis caching?**
A: Add `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package and use `IDistributedCache`.

**Q: Is the example API production-ready?**
A: No. It's educational. Production needs: HTTPS strict mode, rate limiting, input sanitization, logging, monitoring.

## Next Steps

1. **Read LEARNING_GUIDE.md** for week-by-week curriculum
2. **Pick a file** and study it completely
3. **Modify the code** - change parameters, measure impact
4. **Build projects** - apply concepts to real applications
5. **Use profilers** - don't guess, measure!

Good luck! 🚀
