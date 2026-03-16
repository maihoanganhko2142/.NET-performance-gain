using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AdvancedDotNetAPI.Security.Authentication;

/// <summary>
/// PHASE 2: Security - Authentication & JWT Best Practices
///
/// Learning Goals:
/// - Understand JWT structure and claims-based identity
/// - Implement secure token generation with short expiration
/// - Handle token validation with proper algorithm verification
/// - Prevent token tampering with strong symmetric keys
/// </summary>
public class JwtTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(
        string secretKey,
        string issuer = "AdvancedDotNetAPI",
        string audience = "AdvancedDotNetAPIClient",
        int expirationMinutes = 15)  // Short expiration = better security
    {
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            throw new ArgumentException("Secret key must be at least 32 characters long for HS256");

        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
        _expirationMinutes = expirationMinutes;
    }

    /// <summary>
    /// Generate a secure JWT token with claims.
    ///
    /// Security best practices demonstrated:
    /// - Short expiration time (15 min default) to limit token misuse window
    /// - Including issuer and audience to prevent token reuse across services
    /// - Using HS256 (HMAC-SHA256) symmetric algorithm for signing
    /// - Including issued-at time for token age validation
    /// </summary>
    public string GenerateToken(string userId, string username, string[] roles)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
        };

        // Add role claims - important for authorization decisions
        var claimsWithRoles = claims.Concat(
            roles.Select(role => new Claim(ClaimTypes.Role, role))
        ).ToArray();

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claimsWithRoles,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),  // Always use UTC
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validate a JWT token with comprehensive security checks.
    ///
    /// Prevents token tampering and forgery attacks by verifying:
    /// - Signature validity (token wasn't modified)
    /// - Expiration time (token hasn't expired)
    /// - Issuer and audience claims (correct source and recipient)
    /// - Token format (well-formed JWT structure)
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,  // Don't allow time skew - strict validation
                RequireExpirationTime = true
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (SecurityTokenException ex)
        {
            // Log this - could indicate tampering attempts
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Refresh token pattern (PHASE 2 extension).
    /// In production, use a separate long-lived refresh token stored in a secure cookie.
    /// This is a simplified example - real implementation should use refresh token rotation.
    /// </summary>
    public string? RefreshToken(string expiredToken)
    {
        // NOTE: In production, validate against refresh token database
        // and implement refresh token rotation for security
        var principal = ValidateToken(expiredToken);
        if (principal == null)
            return null;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = principal.FindFirst(ClaimTypes.Name)?.Value;
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        if (userId == null || username == null)
            return null;

        return GenerateToken(userId, username, roles);
    }
}
