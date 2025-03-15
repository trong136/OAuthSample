using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthSample.Authorization;
using OAuthSample.Models;
using OAuthSample.Services;

namespace OAuthSample.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly RoleRepository _roleRepository;
    private readonly PermissionService _permissionService;
    
    public UsersController(
        UserRepository userRepository, 
        RoleRepository roleRepository,
        PermissionService permissionService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionService = permissionService;
    }
    
    [HttpGet]
    public IActionResult GetAllUsers()
    {
        if (!_permissionService.HasPermission(User, "users.view"))
        {
            return Forbid();
        }
        
        var users = _userRepository.GetAllUsers();
        
        // Don't return passwords in the response
        var sanitizedUsers = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.CreatedAt,
            u.UpdatedAt
        });
        
        return Ok(sanitizedUsers);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetUserById(Guid id)
    {
        if (!_permissionService.HasPermission(User, "users.view"))
        {
            return Forbid();
        }
        
        var user = _userRepository.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }
        
        // Don't return password in the response
        var sanitizedUser = new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt,
            Roles = _roleRepository.GetUserRoles(id)
        };
        
        return Ok(sanitizedUser);
    }
    
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        if (!_permissionService.HasPermission(User, "users.create"))
        {
            return Forbid();
        }
        
        if (_userRepository.GetUserByUsername(user.Username) != null)
        {
            return BadRequest("Username already exists");
        }
        
        var createdUser = _userRepository.CreateUser(user);
        
        // Don't return password in the response
        var sanitizedUser = new
        {
            createdUser.Id,
            createdUser.Username,
            createdUser.Email,
            createdUser.FirstName,
            createdUser.LastName,
            createdUser.IsActive,
            createdUser.CreatedAt,
            createdUser.UpdatedAt
        };
        
        return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, sanitizedUser);
    }
    
    [HttpPut("{id}")]
    public IActionResult UpdateUser(Guid id, [FromBody] User updatedUser)
    {
        if (!_permissionService.HasPermission(User, "users.edit"))
        {
            return Forbid();
        }
        
        var result = _userRepository.UpdateUser(id, updatedUser);
        if (result == null)
        {
            return NotFound();
        }
        
        // Don't return password in the response
        var sanitizedUser = new
        {
            result.Id,
            result.Username,
            result.Email,
            result.FirstName,
            result.LastName,
            result.IsActive,
            result.CreatedAt,
            result.UpdatedAt
        };
        
        return Ok(sanitizedUser);
    }
    
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(Guid id)
    {
        if (!_permissionService.HasPermission(User, "users.delete"))
        {
            return Forbid();
        }
        
        var result = _userRepository.DeleteUser(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
    [HttpPut("{id}/password")]
    public IActionResult UpdatePassword(Guid id, [FromBody] PasswordUpdateRequest request)
    {
        // Allow users to update their own password without special permission
        var currentUserId = User.FindFirst("userId")?.Value;
        if (currentUserId != id.ToString() && !_permissionService.HasPermission(User, "users.edit"))
        {
            return Forbid();
        }
        
        var result = _userRepository.UpdateUserPassword(id, request.NewPassword);
        if (result == null)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
    [HttpGet("{id}/roles")]
    public IActionResult GetUserRoles(Guid id)
    {
        if (!_permissionService.HasPermission(User, "users.view") && 
            !_permissionService.HasPermission(User, "roles.view"))
        {
            return Forbid();
        }
        
        var user = _userRepository.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }
        
        var roles = _roleRepository.GetUserRoles(id);
        return Ok(roles);
    }
    
    [HttpPost("{userId}/roles/{roleId}")]
    public IActionResult AddRoleToUser(Guid userId, Guid roleId)
    {
        if (!_permissionService.HasPermission(User, "users.edit") && 
            !_permissionService.HasPermission(User, "roles.edit"))
        {
            return Forbid();
        }
        
        var user = _userRepository.GetUserById(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        var role = _roleRepository.GetRoleById(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }
        
        // Check if the user already has this role
        var userRoles = _roleRepository.GetUserRoles(userId);
        if (userRoles.Any(r => r.Id == roleId))
        {
            return BadRequest("User already has this role");
        }
        
        var userRole = _roleRepository.AssignRoleToUser(userId, roleId);
        return Ok(userRole);
    }
    
    [HttpDelete("{userId}/roles/{roleId}")]
    public IActionResult RemoveRoleFromUser(Guid userId, Guid roleId)
    {
        if (!_permissionService.HasPermission(User, "users.edit") && 
            !_permissionService.HasPermission(User, "roles.edit"))
        {
            return Forbid();
        }
        
        var user = _userRepository.GetUserById(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        var role = _roleRepository.GetRoleById(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }
        
        var result = _roleRepository.RemoveRoleFromUser(userId, roleId);
        if (!result)
        {
            return NotFound("User does not have this role");
        }
        
        return NoContent();
    }
}

public class PasswordUpdateRequest
{
    public string NewPassword { get; set; } = string.Empty;
} 