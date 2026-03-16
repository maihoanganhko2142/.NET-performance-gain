# Advanced .NET Security & Performance Learning Environment - Index

Welcome! This is your complete learning environment for mastering advanced .NET techniques.

---

## 📖 Documentation (Read These First)

Start with these in order:

1. **[QUICK_START.md](QUICK_START.md)** ⭐ START HERE
   - Project overview
   - Quick setup instructions
   - File organization guide
   - 15 minutes to understand the project

2. **[LEARNING_GUIDE.md](LEARNING_GUIDE.md)** 📚 WEEK-BY-WEEK PLAN
   - Week 1-2: Performance Foundations
   - Week 3-4: Security Fundamentals
   - Week 5+: Advanced Topics
   - Exercises and assessment
   - 45 minutes to plan your study

3. **[DETAILED_ROADMAP.md](DETAILED_ROADMAP.md)** 🎓 DEEP EXPLANATIONS
   - In-depth concept explanations
   - Real code examples
   - Why each pattern matters
   - Common mistakes and gotchas
   - 2-3 hours for complete understanding

4. **[REFERENCE_GUIDE.md](REFERENCE_GUIDE.md)** ⚡ QUICK LOOKUP
   - Common patterns (copy-paste ready)
   - Decision trees
   - Gotchas & mistakes
   - Tool reference
   - Use when you forget something

---

## 🎯 Learning Modules (Study in Order)

### Phase 1: Performance (Weeks 1-4)

#### Module 1.1: Async Patterns
- **File**: [Performance/Async/AsyncPatterns.cs](Performance/Async/AsyncPatterns.cs)
- **Topics**: async/await, ValueTask, buffer pooling, stackalloc
- **Time**: 2 hours reading + 1 hour exercises
- **Prerequisite**: C# async/await basics
- **Exercise**: Implement AsyncLockingCache

#### Module 1.2: Caching Strategies
- **File**: [Performance/Caching/CachingStrategies.cs](Performance/Caching/CachingStrategies.cs)
- **Topics**: Cache-aside, LRU eviction, expiration, thundering herd
- **Time**: 1.5 hours reading + 1 hour exercises
- **Prerequisite**: Understand async patterns
- **Exercise**: Add distributed cache fallback (in-memory → Redis)

#### Module 1.3: Benchmarking & Profiling
- **File**: [Benchmarks/PerformanceBenchmarks.cs](Benchmarks/PerformanceBenchmarks.cs)
- **Topics**: BenchmarkDotNet, measuring allocations, profiling tools
- **Time**: 1 hour reading + 2 hours hands-on
- **Prerequisite**: Understand caching and async
- **Exercise**: Profile your application, find bottlenecks

---

### Phase 2: Security (Weeks 3-6)

#### Module 2.1: Input Validation
- **File**: [Security/Validation/InputValidator.cs](Security/Validation/InputValidator.cs)
- **Topics**: Whitelist validation, injection prevention, XSS, SQL injection
- **Time**: 1.5 hours reading + 1 hour exercises
- **Prerequisite**: None (can study parallel with Phase 1)
- **Exercise**: Build validator catching 10 OWASP Top 10 attacks

#### Module 2.2: Cryptography & Hashing
- **File**: [Security/Cryptography/PasswordHasher.cs](Security/Cryptography/PasswordHasher.cs)
- **Topics**: PBKDF2, password hashing, timing-safe comparison, encryption
- **Time**: 2 hours reading + 1 hour exercises
- **Prerequisite**: Understand input validation
- **Exercise**: Implement secure password reset flow

#### Module 2.3: Authentication (JWT)
- **File**: [Security/Authentication/JwtTokenService.cs](Security/Authentication/JwtTokenService.cs)
- **Topics**: JWT structure, token generation, validation, refresh tokens
- **Time**: 1.5 hours reading + 1 hour exercises
- **Prerequisite**: Understand cryptography
- **Exercise**: Implement OAuth2 integration

---

### Phase 3: Integration (Week 7+)

#### Module 3.1: Full API Example
- **File**: [Controllers/SecurityAndPerformanceController.cs](Controllers/SecurityAndPerformanceController.cs)
- **Topics**: All concepts combined in working API
- **Time**: 1 hour reading + 2 hours modification
- **Prerequisite**: Complete Phase 1 & 2
- **Exercise**: Add new endpoint with security + performance

---

## 🔍 Quick Access by Topic

### Performance Topics

| Need | File | Section |
|------|------|---------|
| Async patterns | AsyncPatterns.cs | `AsyncBestPractices` class |
| ValueTask optimization | AsyncPatterns.cs | `GetCachedDataAsync` method |
| Buffer pooling | AsyncPatterns.cs | `BufferPooling` class |
| In-memory caching | CachingStrategies.cs | `InMemoryCache<T>` class |
| Async-safe locking | CachingStrategies.cs | `AsyncLockingCache<T>` class |
| Benchmarking examples | PerformanceBenchmarks.cs | All benchmark classes |

### Security Topics

| Need | File | Section |
|------|------|---------|
| Input validation | InputValidator.cs | All `Validate*` methods |
| Injection prevention | InputValidator.cs | `Contains*` methods |
| Password hashing | PasswordHasher.cs | `HashPassword` method |
| Password verification | PasswordHasher.cs | `VerifyPassword` method |
| Encryption at rest | PasswordHasher.cs | `EncryptSensitiveData` method |
| JWT token generation | JwtTokenService.cs | `GenerateToken` method |
| JWT token validation | JwtTokenService.cs | `ValidateToken` method |
| Real API example | SecurityAndPerformanceController.cs | All endpoints |

### Learning Resources

| Resource | File | Use Case |
|----------|------|----------|
| Setup guide | QUICK_START.md | Getting started (15 min) |
| Week-by-week plan | LEARNING_GUIDE.md | Study schedule (45 min) |
| Detailed explanations | DETAILED_ROADMAP.md | Deep learning (2-3 hours) |
| Pattern reference | REFERENCE_GUIDE.md | Quick lookup (on demand) |
| This file | INDEX.md | Navigation (now!) |

---

## ✅ Study Progression

### Level 1: Foundations (Weeks 1-2)

**Goals**:
- [ ] Understand async/await execution model
- [ ] Know when to use async vs sync
- [ ] Explain why sync-over-async is bad
- [ ] Understand buffer pooling and Span<T>

**Files to read**:
1. [QUICK_START.md](QUICK_START.md)
2. [Performance/Async/AsyncPatterns.cs](Performance/Async/AsyncPatterns.cs)

**Exercises**:
- Modify AsyncPatterns.cs code samples
- Run benchmarks comparing sync vs async
- Implement small async method with proper error handling

**Success Criteria**:
- Can explain async/await to someone new
- Can identify sync-over-async bugs in code

---

### Level 2: Performance Optimization (Weeks 3-4)

**Goals**:
- [ ] Implement cache-aside pattern
- [ ] Understand cache invalidation strategies
- [ ] Benchmark code with BenchmarkDotNet
- [ ] Identify N+1 query problems

**Files to read**:
1. [Performance/Caching/CachingStrategies.cs](Performance/Caching/CachingStrategies.cs)
2. [Benchmarks/PerformanceBenchmarks.cs](Benchmarks/PerformanceBenchmarks.cs)
3. [DETAILED_ROADMAP.md](DETAILED_ROADMAP.md) Section 1.3 & 1.4

**Exercises**:
- Implement AsyncLockingCache from scratch
- Create benchmarks for string operations
- Profile a real application with dotTrace
- Fix one performance issue in your code

**Success Criteria**:
- Can explain cache-aside pattern
- Can write effective benchmarks
- Can use profiler to identify bottlenecks

---

### Level 3: Security Fundamentals (Weeks 5-6)

**Goals**:
- [ ] Validate all user input using whitelist approach
- [ ] Hash passwords securely with PBKDF2
- [ ] Generate and validate JWT tokens
- [ ] Understand OWASP Top 10

**Files to read**:
1. [QUICK_START.md](QUICK_START.md) Security section
2. [Security/Validation/InputValidator.cs](Security/Validation/InputValidator.cs)
3. [Security/Cryptography/PasswordHasher.cs](Security/Cryptography/PasswordHasher.cs)
4. [Security/Authentication/JwtTokenService.cs](Security/Authentication/JwtTokenService.cs)
5. [DETAILED_ROADMAP.md](DETAILED_ROADMAP.md) Section 2.1-2.3

**Exercises**:
- Build form validator catching injection attacks
- Implement password reset flow
- Generate JWT tokens and verify them
- Perform security audit on your API

**Success Criteria**:
- Can explain why plaintext passwords are dangerous
- Can identify and prevent common injection attacks
- Can design authentication flow using JWT

---

### Level 4: Integration & Real-World Systems (Week 7+)

**Goals**:
- [ ] Combine security + performance in real APIs
- [ ] Design production-ready systems
- [ ] Build complete features (auth, caching, validation)
- [ ] Think about security AND performance together

**Files to read**:
1. [Controllers/SecurityAndPerformanceController.cs](Controllers/SecurityAndPerformanceController.cs)
2. [LEARNING_GUIDE.md](LEARNING_GUIDE.md) Phase 3+
3. [REFERENCE_GUIDE.md](REFERENCE_GUIDE.md) for quick lookup

**Exercises**:
- Add registration endpoint (validation + hashing + JWT)
- Add profile endpoint (auth + caching + performance)
- Add admin endpoint (auth + authorization + input validation)
- Implement refresh token rotation
- Add rate limiting to prevent brute force

**Success Criteria**:
- Can design complete authentication flows
- Can build secure AND performant APIs
- Can identify and fix security + performance issues

---

### Level 5: Mastery & Specialization (Week 8+)

Choose your path:

**Path A: Performance Master**
- [ ] Implement distributed caching (Redis)
- [ ] Optimize database queries
- [ ] Load test application with k6
- [ ] Reduce p99 latency by 50%+

**Path B: Security Architect**
- [ ] Implement OAuth2 with external providers
- [ ] Add HTTPS certificate pinning
- [ ] Design zero-trust architecture
- [ ] Pass security audit

**Path C: Full-Stack Engineer**
- [ ] Build complete application feature
- [ ] Include auth, validation, caching, logging
- [ ] Deploy to production
- [ ] Monitor and optimize

**Path D: Teaching & Mentoring**
- [ ] Teach concepts to team members
- [ ] Build internal training materials
- [ ] Review and mentor security/performance improvements
- [ ] Establish best practices

---

## 🚀 Running the Project

### Setup

```bash
# Build
dotnet build AdvancedDotNetAPI.csproj

# Run API
dotnet run

# Run tests (if applicable)
dotnet test
```

### Test Endpoints

```bash
# Login
curl -X POST https://localhost:5001/api/v1/securityandperformance/login \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"alice_password_123"}'

# Use returned token to access protected endpoint
TOKEN="<token from login>"
curl https://localhost:5001/api/v1/securityandperformance/profile \
  -H "Authorization: Bearer $TOKEN"
```

### Run Benchmarks

```bash
dotnet run -c Release
# Results in: BenchmarkDotNet.Artifacts/
```

---

## 📋 File Structure

```
AdvancedDotNetAPI/
├── Documentation/
│   ├── INDEX.md                       ← You are here
│   ├── QUICK_START.md                 ← Start here (15 min)
│   ├── LEARNING_GUIDE.md              ← Study schedule (45 min)
│   ├── DETAILED_ROADMAP.md            ← Deep dives (2-3 hours)
│   └── REFERENCE_GUIDE.md             ← Quick lookup
│
├── Security/
│   ├── Authentication/
│   │   └── JwtTokenService.cs         ← JWT tokens (Module 2.3)
│   ├── Cryptography/
│   │   └── PasswordHasher.cs          ← Password hashing (Module 2.2)
│   └── Validation/
│       └── InputValidator.cs          ← Input sanitization (Module 2.1)
│
├── Performance/
│   ├── Async/
│   │   └── AsyncPatterns.cs           ← Async best practices (Module 1.1)
│   ├── Caching/
│   │   └── CachingStrategies.cs       ← Caching patterns (Module 1.2)
│   └── Profiling/
│       └── (Tools guide in comments)
│
├── Benchmarks/
│   └── PerformanceBenchmarks.cs       ← BenchmarkDotNet examples (Module 1.3)
│
├── Controllers/
│   └── SecurityAndPerformanceController.cs  ← Real API (Module 3.1)
│
└── AdvancedDotNetAPI.csproj           ← Project file
```

---

## 🎓 Assessment Questions

### Can you answer these?

**Async/Await**:
- [ ] Why does async not make code faster?
- [ ] What's the difference between Task and ValueTask?
- [ ] What does `await` do at the statement level?

**Caching**:
- [ ] What is the cache-aside pattern?
- [ ] How do you prevent the thundering herd problem?
- [ ] When should you invalidate cache?

**Input Validation**:
- [ ] Why is whitelist validation better than blacklist?
- [ ] What's the difference between SQL injection and XSS?
- [ ] How do you prevent path traversal attacks?

**Passwords**:
- [ ] Why should passwords be hashed?
- [ ] What's the purpose of a salt?
- [ ] Why should password verification be constant-time?

**JWT**:
- [ ] What are the three parts of a JWT?
- [ ] Why should access tokens expire quickly?
- [ ] How is a JWT signature created and verified?

**Full Integration**:
- [ ] How do security and performance work together?
- [ ] What are the layers of defense-in-depth?
- [ ] What's an appropriate token expiration time?

---

## 💡 Tips for Success

1. **Read the comments first** - They explain "why", not just "what"
2. **Modify the code** - Change parameters and see what breaks
3. **Run benchmarks** - Don't guess, measure!
4. **Use profilers** - See where time is actually spent
5. **Write tests** - Especially for security code
6. **Build projects** - Apply concepts to real problems
7. **Share knowledge** - Teaching others deepens your understanding

---

## 🔗 External Resources

### Documentation
- [Microsoft Docs - Async/Await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Microsoft Docs - Security](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)

### Tools
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)
- [JetBrains dotTrace](https://www.jetbrains.com/profiler/)
- [OWASP ZAP](https://www.zaproxy.org/)

### Online
- [JWT Debugger](https://jwt.io)
- [Regex Tester](https://regex101.com)
- [Hash Cracking](https://crackstation.net)

---

## ❓ Frequently Asked Questions

**Q: How long will this take to complete?**
A: 6-8 weeks for solid foundation, 3-6 months for mastery. Work at your pace!

**Q: Can I skip Phase 1 (Performance)?**
A: Not recommended. Async is fundamental to modern .NET.

**Q: Can I skip Phase 2 (Security)?**
A: Absolutely not. Security is non-negotiable.

**Q: Should I read all documentation?**
A: Start with QUICK_START.md and LEARNING_GUIDE.md, use REFERENCE_GUIDE.md as needed.

**Q: How do I practice?**
A: Modify the code, write benchmarks, build projects applying the concepts.

**Q: What if I get stuck?**
A: Re-read DETAILED_ROADMAP.md for that topic, check REFERENCE_GUIDE.md for patterns.

---

## 🎯 Next Steps

1. **Right now** (5 min): Read QUICK_START.md
2. **Today** (1 hour): Build and run the project
3. **This week** (5 hours): Read LEARNING_GUIDE.md and AsyncPatterns.cs
4. **Next week** (10 hours): Complete Module 1.1 exercises
5. **Ongoing**: Use REFERENCE_GUIDE.md for quick lookup

---

*Last Updated: March 2026*
*Ready to start? Open [QUICK_START.md](QUICK_START.md) next! 🚀*
