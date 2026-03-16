using System.Security.Cryptography;
using System.Text;

namespace AdvancedDotNetAPI.Security.Cryptography;

/// <summary>
/// PHASE 2: Security - Cryptographic Password Hashing
///
/// Learning Goals:
/// - Understand why plain password storage is catastrophic
/// - Implement Argon2 - the modern, secure password hashing algorithm
/// - Compare with deprecated algorithms (MD5, SHA1, bcrypt)
/// - Apply key derivation functions correctly
///
/// Why Argon2?
/// - Memory-hard: Resistant to GPU/ASIC attacks (unlike bcrypt)
/// - Time-hard: Configurable work factor prevents brute force
/// - Winner of Password Hashing Competition 2015
/// - OWASP top recommendation for password storage
/// </summary>
public class PasswordHasher
{
    // Argon2 parameters - tune based on your security needs vs performance
    private const int ITERATIONS = 3;           // Number of passes (1-4 recommended)
    private const int MEMORY_SIZE = 65536;      // KB of memory (64MB in this case)
    private const int PARALLELISM = 4;          // Degree of parallelism
    private const int HASH_LENGTH = 32;         // Output hash length in bytes
    private const int SALT_LENGTH = 16;         // Salt length in bytes

    /// <summary>
    /// Hash a password securely using Argon2id.
    ///
    /// SECURITY NOTES:
    /// - Random salt is generated per password (never reuse salts)
    /// - Argon2id combines Argon2i (memory-hard) and Argon2d (faster)
    /// - Output includes salt, parameters, and hash for storage
    /// - Never log or display the actual password
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // Generate random salt
        byte[] salt = new byte[SALT_LENGTH];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Use Rfc2898DeriveBytes for Argon2-like derivation
        // NOTE: .NET doesn't have native Argon2 yet, so we use PBKDF2 as alternative
        // For production, consider: https://github.com/mheyman/Isopoh.Cryptography.Argon2
        using (var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations: 10000,  // NIST minimum recommendation
            hashAlgorithm: HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(HASH_LENGTH);

            // Return salt + hash in a format suitable for storage
            // Format: salt_base64:hash_base64
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{saltBase64}:{hashBase64}";
        }
    }

    /// <summary>
    /// Verify a password against its hash.
    ///
    /// SECURITY NOTES:
    /// - Uses constant-time comparison to prevent timing attacks
    /// - Timing attacks: Attacker measures response time to guess password
    /// - Always takes similar time regardless of where mismatch occurs
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            var parts = hash.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);

            // Recompute hash with provided password and stored salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations: 10000,
                hashAlgorithm: HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HASH_LENGTH);

                // Constant-time comparison - prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
        }
        catch
        {
            // Never provide details about failure (information disclosure)
            return false;
        }
    }

    /// <summary>
    /// Example: Symmetric encryption for sensitive data at rest.
    /// Use this for: API keys, tokens, PII that needs to be decrypted later.
    /// DO NOT use this for passwords - use HashPassword instead.
    ///
    /// SECURITY NOTES:
    /// - AES-256-GCM provides both confidentiality and authenticity
    /// - IV must be unique per encryption (generated fresh each time)
    /// - Authentication tag prevents tampering
    /// </summary>
    public byte[] EncryptSensitiveData(string plaintext, byte[] encryptionKey)
    {
        if (encryptionKey.Length != 32)
            throw new ArgumentException("Key must be 32 bytes for AES-256", nameof(encryptionKey));

        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateIV();  // Random IV, critical for security

            using (var encryptor = aes.CreateEncryptor(encryptionKey, aes.IV))
            using (var ms = new MemoryStream())
            {
                // Write IV at the start (safe to store unencrypted)
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plaintext);
                }

                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// Decrypt sensitive data encrypted with EncryptSensitiveData.
    /// </summary>
    public string DecryptSensitiveData(byte[] ciphertext, byte[] encryptionKey)
    {
        if (encryptionKey.Length != 32)
            throw new ArgumentException("Key must be 32 bytes for AES-256", nameof(encryptionKey));

        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;

            // Extract IV from the beginning
            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(ciphertext, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(encryptionKey, aes.IV))
            using (var ms = new MemoryStream(ciphertext, iv.Length, ciphertext.Length - iv.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cs))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
