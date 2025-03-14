using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OAuthSample.Configuration;
using OAuthSample.Models;

namespace OAuthSample.Services;

/// <summary>
/// Handles authentication business logic
/// This service is scoped (per-request) and uses the singleton TokenRepository for storage
/// </summary>
public class AuthService
{
    private readonly JwtConfig _jwtConfig;
    private readonly IConfiguration _configuration;
    private readonly TokenRepository _tokenRepository;

    public AuthService(IConfiguration configuration, TokenRepository tokenRepository)
    {
        _configuration = configuration;
        _tokenRepository = tokenRepository;
        _jwtConfig = configuration.GetSection("JwtConfig").Get<JwtConfig>() ?? new JwtConfig();
        
        Console.WriteLine("New AuthService instance created");
    }

    public bool ValidateUser(User user)
    {
        // Hardcoded user credentials for demo purposes
        return user.Username == "admin" && user.Password == "password123";
    }

    public string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    // Generate a refresh token
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    // Create and store a refresh token
    public RefreshToken CreateRefreshToken(string username)
    {
        var sessionId = Guid.NewGuid().ToString();
        
        var refreshToken = new RefreshToken
        {
            Username = username,
            Token = GenerateRefreshToken(),
            SessionId = sessionId,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationInDays)
        };
        
        // Store token in the repository
        _tokenRepository.StoreToken(refreshToken);
        
        return refreshToken;
    }
    
    // Validate a refresh token
    public RefreshToken? ValidateRefreshToken(string token)
    {
        return _tokenRepository.FindValidToken(token);
    }
    
    // Revoke a refresh token
    public bool RevokeToken(string token)
    {
        return _tokenRepository.RevokeToken(token);
    }
    
    // Get all active refresh tokens for a user
    public IEnumerable<RefreshToken> GetActiveRefreshTokens(string username)
    {
        return _tokenRepository.GetTokensForUser(username)
            .Where(t => !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow);
    }
    
    // Revoke a specific session
    public bool RevokeSession(string sessionId)
    {
        return _tokenRepository.RevokeSessionTokens(sessionId);
    }
    
    // Revoke all sessions for a specific user
    public bool RevokeAllUserSessions(string username)
    {
        return _tokenRepository.RevokeUserTokens(username);
    }
    
    // Get all sessions for a specific user
    public IEnumerable<object> GetUserSessions(string username)
    {
        return _tokenRepository.GetUserSessions(username);
    }
} 