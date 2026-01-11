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
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
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
