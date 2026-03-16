using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace AdvancedDotNetAPI.Security.Validation;

/// <summary>
/// PHASE 2: Security - Input Validation & Injection Prevention
///
/// Learning Goals:
/// - Understand common injection attacks (SQL, XSS, command injection)
/// - Implement whitelist-based validation
/// - Prevent regular expression denial of service (ReDoS)
/// - Sanitize user input at system boundaries
///
/// OWASP Top 10 Coverage:
/// - A03:2021 – Injection (SQL, Command, LDAP)
/// - A07:2021 – Cross-Site Scripting (XSS)
/// - A01:2021 – Broken Access Control (validate permissions)
/// </summary>
public class InputValidator
{
    /// <summary>
    /// Whitelist pattern validation - the most secure approach.
    /// Only allow known-good patterns, reject everything else.
    /// </summary>
    public static class ValidationPatterns
    {
        // Email: Simple pattern (use FluentValidation for production)
        public static readonly Regex EmailPattern = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(100));  // Timeout prevents ReDoS

        // Username: Alphanumeric, dash, underscore only
        public static readonly Regex UsernamePattern = new(
            @"^[a-zA-Z0-9_-]{3,32}$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(50));

        // URL: Basic HTTP/HTTPS only (prevent javascript: and data: URLs)
        public static readonly Regex UrlPattern = new(
            @"^https?:\/\/[a-zA-Z0-9\-._~:/?#\[\]@!$&'()*+,;=]+$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));

        // Numeric: Positive integers only
        public static readonly Regex NumericPattern = new(
            @"^\d+$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(50));
    }

    /// <summary>
    /// Validate email address with whitelist pattern.
    /// Pattern is designed to be strict to prevent injection.
    /// </summary>
    public static bool ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        if (email.Length > 255)  // Email max length per RFC 5321
            return false;

        try
        {
            return ValidationPatterns.EmailPattern.IsMatch(email);
        }
        catch (RegexMatchTimeoutException)
        {
            // ReDoS attempt detected - timeout occurred
            Console.WriteLine("ReDoS detected in email validation");
            return false;
        }
    }

    /// <summary>
    /// Validate username - prevent path traversal and injection.
    /// Whitelist approach: only allow safe characters.
    /// </summary>
    public static bool ValidateUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        try
        {
            return ValidationPatterns.UsernamePattern.IsMatch(username);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Prevent SQL Injection by checking for common SQL keywords.
    /// BEST PRACTICE: Use parameterized queries instead (this is defense-in-depth).
    ///
    /// In production:
    /// - Never concatenate user input into SQL queries
    /// - Always use parameterized queries/prepared statements
    /// - Use ORMs with proper parameter handling
    /// </summary>
    public static bool ContainsSuspiciousSqlKeywords(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Dangerous SQL keywords to block
        string[] suspiciousKeywords = new[]
        {
            "DROP", "DELETE", "INSERT", "UPDATE", "UNION", "SELECT",
            "EXEC", "EXECUTE", "SCRIPT", "DECLARE", "CAST", "--", ";", "/*", "*/"
        };

        var upperInput = input.ToUpperInvariant();
        return suspiciousKeywords.Any(keyword => upperInput.Contains(keyword));
    }

    /// <summary>
    /// Prevent XSS attacks by checking for HTML/JavaScript patterns.
    /// BEST PRACTICE: HTML encode output (this is input-layer defense-in-depth).
    ///
    /// XSS prevention:
    /// 1. Input: Validate/sanitize (this method)
    /// 2. Output: HTML encode when rendering to HTML
    /// 3. Context: Use CSP headers, SameSite cookies
    /// </summary>
    public static bool ContainsSuspiciousHtmlPatterns(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Dangerous HTML/JS patterns
        string[] suspiciousPatterns = new[]
        {
            "<script", "javascript:", "onerror=", "onclick=", "onload=",
            "<iframe", "<embed", "<object", "eval(", "expression("
        };

        var lowerInput = input.ToLowerInvariant();
        return suspiciousPatterns.Any(pattern => lowerInput.Contains(pattern));
    }

    /// <summary>
    /// Prevent path traversal attacks.
    /// Validate that paths don't escape intended directory.
    /// </summary>
    public static bool ContainsPathTraversalPatterns(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Common path traversal patterns
        return input.Contains("..") || input.Contains("..\\") ||
               input.Contains("./") || input.Contains(".\\") ||
               input.Contains("/etc/") || input.Contains("C:\\");
    }

    /// <summary>
    /// Comprehensive input validation with multiple checks.
    /// Returns detailed validation result.
    /// </summary>
    public static ValidationResult ValidateUserInput(string input, string fieldName = "input")
    {
        var errors = new List<string>();

        // Length check
        if (input.Length > 1000)
            errors.Add($"{fieldName} exceeds maximum length of 1000 characters");

        // Whitespace only
        if (string.IsNullOrWhiteSpace(input))
            errors.Add($"{fieldName} cannot be empty or whitespace");

        // SQL injection check
        if (ContainsSuspiciousSqlKeywords(input))
            errors.Add($"{fieldName} contains suspicious SQL keywords");

        // XSS check
        if (ContainsSuspiciousHtmlPatterns(input))
            errors.Add($"{fieldName} contains suspicious HTML/JavaScript patterns");

        // Path traversal check
        if (ContainsPathTraversalPatterns(input))
            errors.Add($"{fieldName} contains path traversal patterns");

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// Result object for validation checks.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
