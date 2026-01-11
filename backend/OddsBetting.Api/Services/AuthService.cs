using System.Security.Cryptography;
using System.Text;
using OddsBetting.Api.Models;

namespace OddsBetting.Api.Services;

public class AuthService
{
    private readonly SessionStorageService _storage;

    public AuthService(SessionStorageService storage)
    {
        _storage = storage;
    }

    public string HashPassword(string password)
    {
        // Use PBKDF2 with random salt for secure password hashing
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
        
        // Combine salt and hash
        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);
        
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var hashBytes = Convert.FromBase64String(storedHash);
        
        // Extract salt
        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);
        
        // Compute hash of entered password with extracted salt
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
        
        // Compare hashes
        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != hash[i])
            {
                return false;
            }
        }
        return true;
    }

    public string GenerateInviteCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }

    public string GenerateSessionToken()
    {
        return Guid.NewGuid().ToString();
    }

    public User? Register(UserRegisterRequest request)
    {
        // Check if email already exists
        if (_storage.GetUserByEmail(request.Email) != null)
        {
            return null;
        }

        // Validate invite code
        if (!_storage.ValidateInviteCode(request.InviteCode))
        {
            return null;
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            InvitedBy = request.InviteCode,
            InviteCode = GenerateInviteCode()
        };

        _storage.AddUser(user);
        return user;
    }

    public (User?, string?) Login(UserLoginRequest request)
    {
        var user = _storage.GetUserByEmail(request.Email);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return (null, null);
        }

        var sessionToken = GenerateSessionToken();
        _storage.CreateSession(sessionToken, user.Id);
        return (user, sessionToken);
    }

    public void Logout(string sessionToken)
    {
        _storage.DeleteSession(sessionToken);
    }

    public User? GetUserFromSession(string? sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken))
        {
            return null;
        }

        var userId = _storage.GetUserIdBySession(sessionToken);
        return userId != null ? _storage.GetUserById(userId) : null;
    }
}
