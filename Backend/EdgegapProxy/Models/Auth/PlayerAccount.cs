using System.ComponentModel.DataAnnotations;

namespace EdgegapProxy.Models.Auth;

/// <summary>
/// Player account model for database
/// </summary>
public class PlayerAccount
{
    [Key]
    public Guid PlayerId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [StringLength(255)]
    public required string PasswordHash { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; }
}

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [StringLength(64, MinimumLength = 8)]
    public required string Password { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

/// <summary>
/// Response model for successful authentication
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public required string PlayerId { get; set; }
    public required string Username { get; set; }
    public required string Token { get; set; }
    public DateTime? LastLogin { get; set; }
}

/// <summary>
/// Response model for token validation
/// </summary>
public class ValidationResponse
{
    public bool Valid { get; set; }
    public string? PlayerId { get; set; }
    public string? Username { get; set; }
}
