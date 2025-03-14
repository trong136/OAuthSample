using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthSample.Services;

namespace OAuthSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthStatusController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthStatusController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("check")]
    public IActionResult CheckAuthStatus()
    {
        // Check if the request has a JWT cookie
        var jwtToken = Request.Cookies["jwt"];
        var refreshToken = Request.Cookies["refresh_token"];
        
        // For debugging - log token presence
        Console.WriteLine($"JWT Token present: {!string.IsNullOrEmpty(jwtToken)}");
        Console.WriteLine($"Refresh Token present: {!string.IsNullOrEmpty(refreshToken)}");
        
        string? username = null;
        string? sessionId = null;
        
        // If there's a valid refresh token, get the user info
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var validatedToken = _authService.ValidateRefreshToken(refreshToken);
            if (validatedToken != null)
            {
                username = validatedToken.Username;
                sessionId = validatedToken.SessionId;
                Console.WriteLine($"Valid refresh token for user: {username}, session: {sessionId}");
            }
        }
        
        // If we have a JWT token and username, user is authenticated
        bool isAuthenticated = !string.IsNullOrEmpty(jwtToken) && !string.IsNullOrEmpty(username);
        
        if (isAuthenticated)
        {
            return Ok(new { isAuthenticated = true, username, sessionId });
        }
        
        return Ok(new { isAuthenticated = false });
    }
} 