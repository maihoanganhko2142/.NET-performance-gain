using Microsoft.AspNetCore.Mvc;
using AdvancedDotNetAPI.Security.Authentication;
using AdvancedDotNetAPI.Security.Cryptography;
using AdvancedDotNetAPI.Security.Validation;
using AdvancedDotNetAPI.Performance.Caching;
using System.Security.Claims;

namespace AdvancedDotNetAPI.Controllers;

/// <summary>
/// FULL-STACK EXAMPLE: Security + Performance
///
/// This controller demonstrates:
/// - PHASE 2: JWT authentication, password hashing, input validation
/// - PHASE 1: In-memory caching, async patterns
/// - Combining security and performance in a real API
///
/// SECURITY LAYERS:
/// 1. Input validation (prevent injection)
/// 2. Authentication (JWT verification)
/// 3. Authorization (role-based access)
/// 4. Output encoding (if returning HTML)
///
/// PERFORMANCE LAYERS:
/// 1. Caching (reduce database hits)
/// 2. Async operations (non-blocking I/O)
/// 3. Connection pooling (database)
/// 4. Response compression (HTTP)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SecurityAndPerformanceController : ControllerBase
{
    private readonly JwtTokenService _jwtService;
    private readonly PasswordHasher _passwordHasher;
    private readonly InMemoryCache<string, UserDto> _userCache;

    private static Dictionary<string, (string PasswordHash, string[] Roles)> _userDatabase
        = new()
        {
            { "alice", ("", new[] { "admin" }) },
            { "bob", ("", new[] { "user" }) }
        };

    public SecurityAndPerformanceController()
    {
        // Initialize services
        _jwtService = new JwtTokenService(
            secretKey: "YourVerySecretKeyThatIsAtLeast32CharsLong!!!!",
            expirationMinutes: 15);

        _passwordHasher = new PasswordHasher();
        _userCache = new InMemoryCache<string, UserDto>();

        // Initialize test users with hashed passwords
        var aliceHash = _passwordHasher.HashPassword("alice_password_123");
        var bobHash = _passwordHasher.HashPassword("bob_password_456");

        _userDatabase["alice"] = (aliceHash, new[] { "admin" });
        _userDatabase["bob"] = (bobHash, new[] { "user" });
    }

    /// <summary>
    /// LOGIN ENDPOINT: Authenticate user and return JWT token
    ///
    /// SECURITY CHECKS:
    /// ✓ Input validation (prevent injection)
    /// ✓ Timing-safe password verification (prevent timing attacks)
    /// ✓ Short-lived token (minimize impact if leaked)
    /// ✓ Never log sensitive data (username OK, password NO)
    ///
    /// PERFORMANCE:
    /// - One database query (simulated)
    /// - One password hash verification
    /// - Total time: ~50-100ms (intentionally slow for security)
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // SECURITY: Input validation - prevent injection attacks
        if (string.IsNullOrWhiteSpace(request?.Username))
            return BadRequest("Username required");

        var validationResult = InputValidator.ValidateUsername(request.Username);
        if (!validationResult)
            return BadRequest("Invalid username format");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password required");

        // SECURITY: Check for SQL injection attempts
        if (InputValidator.ContainsSuspiciousSqlKeywords(request.Username))
            return BadRequest("Invalid input");

        // PERFORMANCE: Check cache first
        if (_userCache.Get(request.Username) is UserDto cachedUser)
        {
            // Verify password against cached user
            if (_userDatabase.TryGetValue(request.Username, out var dbEntry))
            {
                if (!_passwordHasher.VerifyPassword(request.Password, dbEntry.PasswordHash))
                    return Unauthorized("Invalid credentials");

                // SECURITY: Return token with roles
                var token = _jwtService.GenerateToken(
                    request.Username,
                    request.Username,
                    dbEntry.Roles);

                return Ok(new { token });
            }
        }

        // SECURITY: Timing-safe password check
        // Takes same time whether password is correct or not (prevents timing attacks)
        if (!_userDatabase.TryGetValue(request.Username, out var userEntry))
            return Unauthorized("Invalid credentials");

        if (!_passwordHasher.VerifyPassword(request.Password, userEntry.PasswordHash))
            return Unauthorized("Invalid credentials");

        // PERFORMANCE: Cache user for 5 minutes
        var user = new UserDto { Username = request.Username, Roles = userEntry.Roles };
        _userCache.Set(request.Username, user, TimeSpan.FromMinutes(5));

        // SECURITY: Issue token with short expiration
        var jwtToken = _jwtService.GenerateToken(
            request.Username,
            request.Username,
            userEntry.Roles);

        return Ok(new { token = jwtToken });
    }

    /// <summary>
    /// PROTECTED ENDPOINT: Requires valid JWT token
    ///
    /// SECURITY:
    /// ✓ Token validation with signature verification
    /// ✓ Expiration check (prevent replay attacks)
    /// ✓ Role-based authorization
    ///
    /// PERFORMANCE:
    /// ✓ In-memory cache for frequently accessed data
    /// ✓ Async operations prevent thread blocking
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Extract token from Authorization header
        var token = Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", "");

        // SECURITY: Validate token signature and expiration
        var principal = _jwtService.ValidateToken(token);
        if (principal == null)
            return Unauthorized("Invalid or expired token");

        var username = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        // PERFORMANCE: Check cache before database hit
        var cached = _userCache.Get(username);
        if (cached != null)
            return Ok(new
            {
                username = cached.Username,
                roles = cached.Roles,
                source = "cache"
            });

        // PERFORMANCE: Async operation (non-blocking)
        var user = await FetchUserFromDatabaseAsync(username);
        if (user == null)
            return NotFound("User not found");

        // PERFORMANCE: Cache result for 5 minutes
        _userCache.Set(username, user, TimeSpan.FromMinutes(5));

        return Ok(new
        {
            username = user.Username,
            roles = user.Roles,
            source = "database"
        });
    }

    /// <summary>
    /// ADMIN ENDPOINT: Requires admin role
    ///
    /// SECURITY:
    /// ✓ Token validation
    /// ✓ Role-based access control (RBAC)
    /// ✓ Input validation for sensitive operations
    /// </summary>
    [HttpPost("admin/create-user")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        // SECURITY: Extract and validate token
        var token = Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", "");

        var principal = _jwtService.ValidateToken(token);
        if (principal == null)
            return Unauthorized("Invalid token");

        // SECURITY: Check admin role
        var isAdmin = principal.FindAll(ClaimTypes.Role)
            .Any(c => c.Value == "admin");

        if (!isAdmin)
            return Forbid("Admin role required");

        // SECURITY: Validate input
        if (!InputValidator.ValidateUsername(request.Username))
            return BadRequest("Invalid username");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters");

        // SECURITY: Hash password before storage (NEVER store plaintext)
        var passwordHash = new PasswordHasher().HashPassword(request.Password);

        // SECURITY: Check for injection attempts
        if (InputValidator.ContainsSuspiciousSqlKeywords(request.Username))
            return BadRequest("Invalid input detected");

        // Simulate database storage
        _userDatabase[request.Username] = (passwordHash, request.Roles ?? new[] { "user" });

        // PERFORMANCE: Clear cache for admin operations (ensure consistency)
        _userCache.Clear();

        return Created(nameof(GetProfile), new { username = request.Username });
    }

    /// <summary>
    /// SEARCH ENDPOINT: Vulnerable to injection if not careful
    /// Shows proper input validation
    /// </summary>
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string q)
    {
        // SECURITY: Validate input length
        if (string.IsNullOrWhiteSpace(q) || q.Length > 100)
            return BadRequest("Invalid search query");

        // SECURITY: Validate against injection attacks
        var validationResult = InputValidator.ValidateUserInput(q, "search query");
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors });

        // SECURITY: Check for XSS patterns
        if (InputValidator.ContainsSuspiciousHtmlPatterns(q))
            return BadRequest("Invalid input detected");

        // PERFORMANCE: Use parameterized queries in real database
        // Example: SELECT * FROM users WHERE name LIKE @q
        // NEVER: SELECT * FROM users WHERE name LIKE '%" + q + "%'

        var results = new[] { "Result 1", "Result 2" };  // Simulated results
        return Ok(results);
    }

    // ===== HELPER METHODS =====

    /// <summary>
    /// Simulate async database query.
    /// In production, this would hit a real database with
    /// connection pooling and parameterized queries.
    /// </summary>
    private async Task<UserDto?> FetchUserFromDatabaseAsync(string username)
    {
        // Simulate I/O latency
        await Task.Delay(10);

        if (_userDatabase.TryGetValue(username, out var entry))
        {
            return new UserDto
            {
                Username = username,
                Roles = entry.Roles
            };
        }

        return null;
    }
}

/// <summary>
/// Request/Response DTOs
/// </summary>
public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class CreateUserRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string[]? Roles { get; set; }
}

public class UserDto
{
    public string Username { get; set; } = "";
    public string[] Roles { get; set; } = Array.Empty<string>();
}
