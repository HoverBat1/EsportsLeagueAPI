using System.ComponentModel.DataAnnotations;

namespace EsportsLeagueApi01.DTOs;

public class RegisterRequest
{
    [Required]
    [MinLength(League.Auth.UsernameLengthMin)]
    [MaxLength(League.Auth.UsernameLengthMax)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(League.Auth.PasswordLengthMin, ErrorMessage = "Password must be at least 6 characters")]
    //[MinLength(League.Auth.PasswordLengthMin, ErrorMessage = League.Auth.PasswordLengthMinErrorMessage)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}