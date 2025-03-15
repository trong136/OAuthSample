using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthSample.Authorization;
using OAuthSample.Models;
using OAuthSample.Services;

namespace OAuthSample.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly RoleRepository _roleRepository;
    private readonly PermissionRepository _permissionRepository;
    private readonly PermissionService _permissionService;
    
    public RolesController(
        RoleRepository roleRepository,
        PermissionRepository permissionRepository,
        PermissionService permissionService)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _permissionService = permissionService;
    }
    
    [HttpGet]
    public IActionResult GetAllRoles()
    {
        if (!_permissionService.HasPermission(User, "roles.view"))
        {
            return Forbid();
        }
        
        var roles = _roleRepository.GetAllRoles();
        return Ok(roles);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetRoleById(Guid id)
    {
        if (!_permissionService.HasPermission(User, "roles.view"))
        {
            return Forbid();
        }
        
        var role = _roleRepository.GetRoleById(id);
        if (role == null)
        {
            return NotFound();
        }
        
        var result = new
        {
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            Permissions = _permissionRepository.GetRolePermissions(id)
        };
        
        return Ok(result);
    }
    
    [HttpPost]
    public IActionResult CreateRole([FromBody] Role role)
    {
        if (!_permissionService.HasPermission(User, "roles.create"))
        {
            return Forbid();
        }
        
        if (_roleRepository.GetRoleByName(role.Name) != null)
        {
            return BadRequest("Role name already exists");
        }
        
        var createdRole = _roleRepository.CreateRole(role);
        return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
    }
    
    [HttpPut("{id}")]
    public IActionResult UpdateRole(Guid id, [FromBody] Role updatedRole)
    {
        if (!_permissionService.HasPermission(User, "roles.edit"))
        {
            return Forbid();
        }
        
        var result = _roleRepository.UpdateRole(id, updatedRole);
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public IActionResult DeleteRole(Guid id)
    {
        if (!_permissionService.HasPermission(User, "roles.delete"))
        {
            return Forbid();
        }
        
        var result = _roleRepository.DeleteRole(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
    [HttpGet("{id}/permissions")]
    public IActionResult GetRolePermissions(Guid id)
    {
        if (!_permissionService.HasPermission(User, "roles.view") && 
            !_permissionService.HasPermission(User, "permissions.view"))
        {
            return Forbid();
        }
        
        var role = _roleRepository.GetRoleById(id);
        if (role == null)
        {
            return NotFound();
        }
        
        var permissions = _permissionRepository.GetRolePermissions(id);
        return Ok(permissions);
    }
    
    [HttpPost("{roleId}/permissions/{permissionId}")]
    public IActionResult AddPermissionToRole(Guid roleId, Guid permissionId)
    {
        if (!_permissionService.HasPermission(User, "roles.edit") && 
            !_permissionService.HasPermission(User, "permissions.assign"))
        {
            return Forbid();
        }
        
        var role = _roleRepository.GetRoleById(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }
        
        var permission = _permissionRepository.GetPermissionById(permissionId);
        if (permission == null)
        {
            return NotFound("Permission not found");
        }
        
        // Check if the role already has this permission
        var rolePermissions = _permissionRepository.GetRolePermissions(roleId);
        if (rolePermissions.Any(p => p.Id == permissionId))
        {
            return BadRequest("Role already has this permission");
        }
        
        var rolePermission = _permissionRepository.AssignPermissionToRole(roleId, permissionId);
        return Ok(rolePermission);
    }
    
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    public IActionResult RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
        if (!_permissionService.HasPermission(User, "roles.edit") && 
            !_permissionService.HasPermission(User, "permissions.assign"))
        {
            return Forbid();
        }
        
        var role = _roleRepository.GetRoleById(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }
        
        var permission = _permissionRepository.GetPermissionById(permissionId);
        if (permission == null)
        {
            return NotFound("Permission not found");
        }
        
        var result = _permissionRepository.RemovePermissionFromRole(roleId, permissionId);
        if (!result)
        {
            return NotFound("Role does not have this permission");
        }
        
        return NoContent();
    }
} 