using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EdgegapProxy.Services;

/// <summary>
/// JWT token generation and validation service
/// </summary>
public class JwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationDays;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not found in configuration");
        _issuer = configuration["Jwt:Issuer"] ?? "wos-game-server";
        _audience = configuration["Jwt:Audience"] ?? "wos-game-client";
        _expirationDays = int.Parse(configuration["Jwt:ExpirationDays"] ?? "7");
        _logger = logger;
    }

    /// <summary>
    /// Generate JWT token for authenticated player
    /// </summary>
    public string GenerateToken(Guid playerId, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

        var claims = new[]
        {
            new Claim("playerId", playerId.ToString()),
            new Claim("username", username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(_expirationDays),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Generated JWT token for player {PlayerId} ({Username})", playerId, username);

        return tokenString;
    }

    /// <summary>
    /// Validate JWT token and extract claims
    /// </summary>
    public (bool Valid, Guid? PlayerId, string? Username) ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Extract claims
            var playerIdClaim = principal.FindFirst("playerId")?.Value;
            var usernameClaim = principal.FindFirst("username")?.Value;

            if (string.IsNullOrEmpty(playerIdClaim) || string.IsNullOrEmpty(usernameClaim))
            {
                _logger.LogWarning("Token missing required claims");
                return (false, null, null);
            }

            if (!Guid.TryParse(playerIdClaim, out var playerId))
            {
                _logger.LogWarning("Invalid playerId format in token");
                return (false, null, null);
            }

            return (true, playerId, usernameClaim);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token has expired");
            return (false, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return (false, null, null);
        }
    }
}
