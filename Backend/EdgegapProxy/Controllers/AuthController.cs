using Microsoft.AspNetCore.Mvc;
using EdgegapProxy.Models.Auth;
using EdgegapProxy.Services;
using BCrypt.Net;

namespace EdgegapProxy.Controllers;

/// <summary>
/// Authentication controller for player accounts
/// Endpoints: /api/auth/register, /api/auth/login, /api/auth/validate
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _database;
    private readonly JwtService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(DatabaseService database, JwtService jwt, ILogger<AuthController> logger)
    {
        _database = database;
        _jwt = jwt;
        _logger = logger;
    }

    /// <summary>
    /// Register new player account
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { error = "Username is required" });
            }

            if (request.Username.Length < 3 || request.Username.Length > 50)
            {
                return BadRequest(new { error = "Username must be between 3 and 50 characters" });
            }

            if (!IsValidUsername(request.Username))
            {
                return BadRequest(new { error = "Username can only contain letters, numbers, and underscores" });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Password is required" });
            }

            if (request.Password.Length < 8 || request.Password.Length > 64)
            {
                return BadRequest(new { error = "Password must be between 8 and 64 characters" });
            }

            // Check if username already exists
            var existing = await _database.GetPlayerAccountByUsernameAsync(request.Username);
            if (existing != null)
            {
                return Conflict(new { error = "Username already exists" });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 10);

            // Create account
            var account = await _database.CreatePlayerAccountAsync(
                request.Username,
                passwordHash,
                request.Email
            );

            if (account == null)
            {
                return StatusCode(500, new { error = "Failed to create account" });
            }

            // Generate JWT token
            var token = _jwt.GenerateToken(account.PlayerId, account.Username);

            _logger.LogInformation("New player account created: {Username} ({PlayerId})", account.Username, account.PlayerId);

            return StatusCode(201, new AuthResponse
            {
                Success = true,
                PlayerId = account.PlayerId.ToString(),
                Username = account.Username,
                Token = token,
                LastLogin = account.LastLogin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Login to existing account
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Username and password are required" });
            }

            // Find account
            var account = await _database.GetPlayerAccountByUsernameAsync(request.Username);
            if (account == null)
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }

            // Verify password
            var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash);
            if (!validPassword)
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }

            // Update last login
            await _database.UpdateLastLoginAsync(account.PlayerId);

            // Generate JWT token
            var token = _jwt.GenerateToken(account.PlayerId, account.Username);

            _logger.LogInformation("Player logged in: {Username} ({PlayerId})", account.Username, account.PlayerId);

            return Ok(new AuthResponse
            {
                Success = true,
                PlayerId = account.PlayerId.ToString(),
                Username = account.Username,
                Token = token,
                LastLogin = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate JWT token
    /// POST /api/auth/validate
    /// Authorization: Bearer {token}
    /// </summary>
    [HttpPost("validate")]
    public IActionResult Validate()
    {
        try
        {
            // Get token from Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "No token provided" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Validate token
            var (valid, playerId, username) = _jwt.ValidateToken(token);

            if (!valid || playerId == null || username == null)
            {
                return Unauthorized(new { error = "Invalid token" });
            }

            return Ok(new ValidationResponse
            {
                Valid = true,
                PlayerId = playerId.ToString(),
                Username = username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate username format (alphanumeric + underscore only)
    /// </summary>
    private bool IsValidUsername(string username)
    {
        return username.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}
