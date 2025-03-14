using OAuthSample.Models;

namespace OAuthSample.Services;

/// <summary>
/// Simulates a database repository for storing and retrieving tokens
/// This class is registered as a singleton to persist tokens across requests
/// </summary>
public class TokenRepository
{
    // In-memory store for refresh tokens (simulating a database)
    private readonly List<RefreshToken> _refreshTokens = new();

    public TokenRepository()
    {
        Console.WriteLine("TokenRepository initialized");
    }

    /// <summary>
    /// Store a new refresh token
    /// </summary>
    public void StoreToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
        Console.WriteLine($"Stored token for user {token.Username} with sessionId {token.SessionId}");
        Console.WriteLine($"Total tokens in repository: {_refreshTokens.Count}");
    }

    /// <summary>
    /// Find a token by its value
    /// </summary>
    public RefreshToken? FindToken(string tokenValue)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == tokenValue);
    }

    /// <summary>
    /// Find a valid (non-revoked, non-expired) token by its value
    /// </summary>
    public RefreshToken? FindValidToken(string tokenValue)
    {
        return _refreshTokens.FirstOrDefault(t => 
            t.Token == tokenValue && 
            !t.IsRevoked && 
            t.ExpiryDate > DateTime.UtcNow);
    }

    /// <summary>
    /// Get all tokens for a specific user
    /// </summary>
    public IEnumerable<RefreshToken> GetTokensForUser(string username)
    {
        return _refreshTokens.Where(t => t.Username == username).ToList();
    }

    /// <summary>
    /// Mark a token as revoked
    /// </summary>
    public bool RevokeToken(string tokenValue)
    {
        var token = FindToken(tokenValue);
        if (token == null)
            return false;
            
        token.IsRevoked = true;
        return true;
    }

    /// <summary>
    /// Revoke all tokens for a specific session
    /// </summary>
    public bool RevokeSessionTokens(string sessionId)
    {
        var tokens = _refreshTokens.Where(t => t.SessionId == sessionId).ToList();
        
        if (!tokens.Any())
            return false;
            
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
        
        return true;
    }

    /// <summary>
    /// Revoke all tokens for a specific user
    /// </summary>
    public bool RevokeUserTokens(string username)
    {
        var tokens = _refreshTokens.Where(t => t.Username == username).ToList();
        
        if (!tokens.Any())
            return false;
            
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
        
        return true;
    }

    /// <summary>
    /// Get all sessions for a specific user
    /// </summary>
    public IEnumerable<object> GetUserSessions(string username)
    {
        // For debugging purposes
        Console.WriteLine($"Fetching sessions for user: {username}");
        Console.WriteLine($"Total tokens in repository: {_refreshTokens.Count}");
        Console.WriteLine($"Tokens for this user: {_refreshTokens.Count(t => t.Username == username)}");
        
        // List all tokens for debugging
        foreach (var token in _refreshTokens.Where(t => t.Username == username))
        {
            Console.WriteLine($"Token: {token.Id}, SessionId: {token.SessionId}, Username: {token.Username}, IsRevoked: {token.IsRevoked}, Expires: {token.ExpiryDate}");
        }
        
        // Group tokens by session ID and get the most recent token for each session
        var sessions = _refreshTokens
            .Where(t => t.Username == username)
            .GroupBy(t => t.SessionId)
            .Select(g => g.OrderByDescending(t => t.CreatedDate).First())
            .Select(t => new 
            {
                SessionId = t.SessionId,
                CreatedDate = t.CreatedDate,
                ExpiryDate = t.ExpiryDate,
                IsActive = !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow
            })
            .ToList();
            
        Console.WriteLine($"Found {sessions.Count} sessions for user {username}");
            
        return sessions;
    }
} 