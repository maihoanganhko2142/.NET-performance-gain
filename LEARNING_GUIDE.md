# .NET Advanced Learning Guide

## Project Overview

This project is a **full-stack learning environment** for mastering advanced .NET techniques with focus on **Security** and **Performance Optimization**.

## 📁 Project Structure

```
.
├── Security/
│   ├── Authentication/          (PHASE 2)
│   │   └── JwtTokenService.cs   JWT token generation & validation
│   ├── Cryptography/            (PHASE 2)
│   │   └── PasswordHasher.cs    PBKDF2, AES encryption, timing-safe hashing
│   └── Validation/              (PHASE 2)
│       └── InputValidator.cs    Input sanitization, injection prevention
├── Performance/
│   ├── Caching/                 (PHASE 1)
│   │   └── CachingStrategies.cs In-memory cache, async-safe locking
│   ├── Async/                   (PHASE 1)
│   │   └── AsyncPatterns.cs     ValueTask, async best practices, pooling
│   └── Profiling/               (PHASE 1)
│       └── (Tools guide included)
├── Benchmarks/
│   └── PerformanceBenchmarks.cs BenchmarkDotNet examples & profiling guide
├── Controllers/
│   └── SecurityAndPerformanceController.cs  Real API demonstrating all concepts
└── LEARNING_GUIDE.md            This file
```

## 🎯 Learning Path

### Phase 1: Performance Foundations (Start Here)

**Goal**: Understand how .NET executes code and optimize for speed

#### Week 1-2: Memory & GC
- Read: `Performance/Async/AsyncPatterns.cs` - Buffer pooling, stackalloc, Span<T>
- Key concepts:
  - Heap vs Stack allocation
  - GC pressure and collection
  - Object pooling patterns
  - ArrayPool<T> usage
- Exercise: Benchmark string concatenation vs StringBuilder vs String.Join

#### Week 3: Async Best Practices
- Read: `Performance/Async/AsyncPatterns.cs` - Full async guide
- Key concepts:
  - async/await execution model
  - ValueTask vs Task
  - Pitfall: sync-over-async
  - Proper async disposal
  - Throttling concurrent operations
- Exercise: Implement AsyncLockingCache and measure thundering herd vs synchronized access

#### Week 4: Benchmarking & Profiling
- Read: `Benchmarks/PerformanceBenchmarks.cs` - All examples
- Use: BenchmarkDotNet to measure your code
- Tools:
  - JetBrains dotTrace (most comprehensive)
  - Windows Performance Toolkit + PerfView (free)
  - Visual Studio Profiler (included)
- Exercise: Profile one of your applications, identify 3 optimization opportunities

#### Week 5: Caching Strategies
- Read: `Performance/Caching/CachingStrategies.cs` - Complete guide
- Key concepts:
  - Cache-aside pattern
  - LRU eviction
  - Expiration handling
  - Cache invalidation strategies
  - Thundering herd prevention
- Exercise: Implement a distributed cache fallback (in-memory → Redis)

---

### Phase 2: Security Foundations (Parallel or After Phase 1)

**Goal**: Write code that resists attacks and handles sensitive data correctly

#### Week 1-2: Input Validation & Injection Prevention
- Read: `Security/Validation/InputValidator.cs` - Comprehensive validation
- Key concepts:
  - Whitelist vs blacklist validation
  - SQL injection prevention (parameterized queries)
  - XSS prevention
  - ReDoS (Regex DoS) attacks
  - Path traversal attacks
- Exercise: Build a form validator that catches all 10 OWASP Top 10 injection types

#### Week 3: Cryptography & Password Hashing
- Read: `Security/Cryptography/PasswordHasher.cs` - All patterns
- Key concepts:
  - Argon2 vs PBKDF2 vs bcrypt
  - Salt handling
  - Timing-safe comparison
  - AES encryption (when to use)
  - Never use: MD5, SHA1, SHA256 for passwords
- Exercise: Implement password reset flow with secure token generation

#### Week 4: Authentication (JWT)
- Read: `Security/Authentication/JwtTokenService.cs` - JWT best practices
- Key concepts:
  - JWT structure (header.payload.signature)
  - Issuer, audience, expiration claims
  - Token validation (signature, expiration, issuer)
  - Refresh token patterns
  - Never trust token contents without verification
- Exercise: Implement OAuth2 / OpenID Connect integration

#### Week 5: Secure API Design
- Read: `Controllers/SecurityAndPerformanceController.cs` - Real examples
- Key concepts:
  - Defense in depth (multiple layers)
  - Principle of least privilege
  - Secure defaults
  - Error handling (don't leak information)
  - HTTPS & security headers
- Exercise: Audit an API you built for security issues

---

### Phase 3: Advanced Topics (Weeks 9+)

#### A. Advanced Caching & Data Access
- Redis (distributed caching)
- Cache warming strategies
- Cache coherency
- Database optimization (N+1 queries, connection pooling)
- Read: DAPPER & EF Core performance guides

#### B. Secure Data Storage
- Field-level encryption in databases
- Key management (HSM, Azure Key Vault)
- PII anonymization
- Data retention policies
- GDPR compliance patterns

#### C. Cryptography Deep Dive
- Public key cryptography (RSA, ECDSA)
- TLS/SSL certificate management
- Certificate pinning
- Key rotation patterns
- Hardware security modules

#### D. High-Performance Systems
- SIMD & vectorization
- Lock-free data structures
- Parallel processing (Parallel.For, Task.Run)
- Network optimization
- Minimize allocations in hot paths

#### E. Secure Architecture
- Service-to-service authentication
- API gateway patterns
- Rate limiting & DDoS protection
- CORS security
- Security headers (CSP, HSTS, X-Frame-Options)

---

## 🏃 Quick Start: Run the Example

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run the API:**
   ```bash
   dotnet run
   ```

3. **Test endpoints:**
   ```bash
   # Login
   curl -X POST https://localhost:5001/api/v1/securityandperformance/login \
     -H "Content-Type: application/json" \
     -d '{"username":"alice","password":"alice_password_123"}'

   # Get Profile (use token from login response)
   curl -X GET https://localhost:5001/api/v1/securityandperformance/profile \
     -H "Authorization: Bearer <token>"

   # Create User (admin only)
   curl -X POST https://localhost:5001/api/v1/securityandperformance/admin/create-user \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <admin-token>" \
     -d '{"username":"charlie","password":"password123","roles":["user"]}'
   ```

4. **Run Benchmarks:**
   ```bash
   dotnet run -c Release -- --benchmarks
   ```
   Output will be in `BenchmarkDotNet.Artifacts/`

---

## 📚 Key Resources

### Official Documentation
- [Microsoft Docs: Security](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [Microsoft Docs: Performance](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/performance)
- [OWASP: Top 10 2021](https://owasp.org/Top10/)

### Books
- **"CLR via C#"** by Jeffrey Richter - Memory, GC, async deep dives
- **"Cryptography and Network Security"** by Stallings - Crypto fundamentals
- **"High-Performance C# and .NET Core"** by Ivo Gabe de Wolff

### Tools
- **BenchmarkDotNet**: Reliable performance testing
- **JetBrains dotTrace**: Comprehensive profiling
- **Roslyn Analyzers**: Static security/performance analysis
- **SonarQube**: Code quality & security scanning
- **OWASP ZAP**: Security testing for APIs

### Free Online
- NIST Cybersecurity Framework: https://www.nist.gov/
- OWASP Security Testing Guide: https://owasp.org/www-project-web-security-testing-guide/
- Cloudflare Learning: https://www.cloudflare.com/learning/

---

## 💡 Best Practices Checklist

### Security
- [ ] All user input validated at system boundaries
- [ ] Passwords hashed with Argon2 or PBKDF2, never stored plaintext
- [ ] Sensitive data encrypted at rest (AES-256)
- [ ] HTTPS enforced everywhere (never HTTP for auth)
- [ ] JWT tokens expire quickly (15-30 min)
- [ ] Refresh tokens stored securely (HTTP-only cookies)
- [ ] SQL queries parameterized (no string concatenation)
- [ ] No sensitive data in logs
- [ ] CORS configured to allow only known origins
- [ ] Security headers set (CSP, HSTS, X-Frame-Options)

### Performance
- [ ] String operations use StringBuilder or LINQ, not concatenation
- [ ] Buffers pooled with ArrayPool<T> for high-throughput paths
- [ ] Async/await used for I/O operations (not sync-over-async)
- [ ] Caching implemented with appropriate TTLs
- [ ] Database queries use indexes and parameterized queries
- [ ] N+1 query problems identified and fixed
- [ ] GC pressure monitored (target: gen2 < 5% overhead)
- [ ] Hot paths profiled and measured with BenchmarkDotNet
- [ ] Connection pooling enabled for external resources
- [ ] Large objects (>85KB) tracked and minimized

---

## 📝 Study Exercises

### Exercise 1: Secure Login
**Goal**: Implement complete login flow with all security checks

```csharp
// TODO: Implement
public IActionResult Login(string username, string password)
{
    // 1. Validate input (use InputValidator)
    // 2. Hash password verification (use PasswordHasher)
    // 3. Generate JWT token (use JwtTokenService)
    // 4. Return token to client
    // 5. Ensure no timing attacks (constant-time comparison)
}
```

### Exercise 2: Caching Layer
**Goal**: Add caching to reduce database load

```csharp
// TODO: Implement
public async Task<User> GetUserAsync(int userId)
{
    // 1. Check in-memory cache
    // 2. If miss, fetch from database
    // 3. Cache result with 5-minute TTL
    // 4. Return user
    // Measure: Compare response time cache hit vs miss
}
```

### Exercise 3: Performance Benchmark
**Goal**: Identify and fix a performance issue

```bash
# 1. Profile your application
# 2. Find hot path (high CPU/allocation)
# 3. Create benchmark with BenchmarkDotNet
# 4. Try optimizations (caching, async, pooling)
# 5. Measure improvement
# Target: 20%+ improvement
```

### Exercise 4: Security Audit
**Goal**: Find and fix security issues

```
Checklist:
- [ ] All inputs validated
- [ ] No hardcoded secrets in code
- [ ] Passwords never logged
- [ ] SQL injection prevented
- [ ] XSS protected
- [ ] CSRF tokens used
- [ ] HTTPS enforced
- [ ] Error messages don't leak info
```

---

## 🎓 Assessment: Am I Ready?

### Phase 1 Complete When You Can:
- [ ] Explain heap vs stack allocation
- [ ] Implement object pooling correctly
- [ ] Use async/await without deadlocks
- [ ] Write benchmarks with BenchmarkDotNet
- [ ] Identify and fix GC pressure issues
- [ ] Implement cache-aside pattern

### Phase 2 Complete When You Can:
- [ ] Hash passwords securely
- [ ] Generate and validate JWT tokens
- [ ] Prevent SQL injection and XSS
- [ ] Implement complete authentication flow
- [ ] Design secure API endpoints
- [ ] Audit code for OWASP Top 10 issues

### Phase 3 Ready When You Can:
- [ ] Design systems handling millions of requests/day
- [ ] Implement encryption at rest and in transit
- [ ] Create zero-trust security architecture
- [ ] Optimize database for performance
- [ ] Scale horizontally with proper caching
- [ ] Respond to security incidents

---

## 🚀 Next Steps

1. **Start with Phase 1** if you want to understand performance first
2. **Start with Phase 2** if you want to understand security first
3. **Do exercises** - don't just read
4. **Measure everything** - use benchmarks and profilers
5. **Build projects** - apply concepts to real applications
6. **Review code** - other's code teaches you new patterns

Good luck! 🎉
