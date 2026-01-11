namespace OddsBetting.Api.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? InviteCode { get; set; }
    public string? InvitedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Balance { get; set; } = 1000m; // Starting balance
}

public class UserLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
}

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string? InviteCode { get; set; }
}
