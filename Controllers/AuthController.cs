using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using OAuthSample.Models;
using OAuthSample.Services;

namespace OAuthSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] User user)
    {
        if (!_authService.ValidateUser(user))
        {
            return Unauthorized();
        }

        // Generate JWT token
        var token = _authService.GenerateJwtToken(user);
        
        // Generate refresh token
        var refreshToken = _authService.CreateRefreshToken(user.Username);
        
        // Set the JWT token in an HTTP-only cookie (short-lived)
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15) // Shorter lifespan for demo
        });
        
        // Set the refresh token in an HTTP-only cookie (long-lived)
        Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7) // 7 days
        });

        return Ok(new { message = "Login successful" });
    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken()
    {
        // Get the refresh token from the cookie
        var refreshToken = Request.Cookies["refresh_token"];
        
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Invalid token" });
            
        // Validate the refresh token
        var validatedToken = _authService.ValidateRefreshToken(refreshToken);
        
        if (validatedToken == null)
            return Unauthorized(new { message = "Invalid or expired token" });
            
        // Create a user object to generate a new JWT token
        var user = new User { Username = validatedToken.Username };
        
        // Generate a new JWT token
        var newJwtToken = _authService.GenerateJwtToken(user);
        
        // Set the new JWT token in an HTTP-only cookie
        Response.Cookies.Append("jwt", newJwtToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15) // Shorter lifespan for demo
        });
        
        return Ok(new { message = "Token refreshed successfully" });
    }
    
    [HttpPost("revoke-token")]
    public IActionResult RevokeToken()
    {
        // Get the refresh token from the cookie
        var refreshToken = Request.Cookies["refresh_token"];
        
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { message = "Token is required" });
            
        // Revoke the token
        var success = _authService.RevokeToken(refreshToken);
        
        if (!success)
            return NotFound(new { message = "Token not found" });
            
        // Clear the cookies
        Response.Cookies.Delete("jwt");
        Response.Cookies.Delete("refresh_token");
        
        return Ok(new { message = "Token revoked" });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Get the refresh token from the cookie
        var refreshToken = Request.Cookies["refresh_token"];
        
        // If there's a refresh token, revoke it
        if (!string.IsNullOrEmpty(refreshToken))
        {
            _authService.RevokeToken(refreshToken);
        }
        
        // Clear the cookies
        Response.Cookies.Delete("jwt");
        Response.Cookies.Delete("refresh_token");
        
        return Ok(new { message = "Logout successful" });
    }
    
    // Get all active sessions for the current user
    [HttpGet("sessions")]
    public IActionResult GetSessions()
    {
        // Since we're using JWT and the User.Identity.Name might not be populated correctly,
        // we'll get the username from the current refresh token
        var refreshToken = Request.Cookies["refresh_token"];
        string username = "";
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var validatedToken = _authService.ValidateRefreshToken(refreshToken);
            if (validatedToken != null)
            {
                username = validatedToken.Username;
            }
        }
        
        // Fallback to User.Identity.Name if available
        if (string.IsNullOrEmpty(username) && User.Identity?.Name != null)
        {
            username = User.Identity.Name;
        }
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        var sessions = _authService.GetUserSessions(username);
        return Ok(sessions);
    }
    
    // Revoke a specific session
    [HttpPost("revoke-session/{sessionId}")]
    public IActionResult RevokeSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return BadRequest(new { message = "Session ID is required" });
        }
        
        var success = _authService.RevokeSession(sessionId);
        
        if (!success)
        {
            return NotFound(new { message = "Session not found" });
        }
        
        return Ok(new { message = "Session revoked successfully" });
    }
    
    // Revoke all sessions for a user (except the current one)
    [HttpPost("revoke-all-sessions")]
    public IActionResult RevokeAllSessions()
    {
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        // Get the current session ID
        var currentToken = Request.Cookies["refresh_token"];
        string currentSessionId = "";
        
        if (!string.IsNullOrEmpty(currentToken))
        {
            var validatedToken = _authService.ValidateRefreshToken(currentToken);
            if (validatedToken != null)
            {
                currentSessionId = validatedToken.SessionId;
            }
        }
        
        // You might want to implement a more selective revocation here
        // For simplicity, we're revoking all sessions for the user
        var success = _authService.RevokeAllUserSessions(username);
        
        if (!success)
        {
            return NotFound(new { message = "No sessions found for the user" });
        }
        
        return Ok(new { message = "All sessions revoked successfully" });
    }
} 