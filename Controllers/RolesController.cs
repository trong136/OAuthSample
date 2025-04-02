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
    
    public RolesController(
        RoleRepository roleRepository,
        PermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }
    
    [HttpGet]
    [RequirePermission("roles.view")]
    public IActionResult GetAllRoles()
    {
        var roles = _roleRepository.GetAllRoles();
        return Ok(roles);
    }
    
    [HttpGet("{id}")]
    [RequirePermission("roles.view")]
    public IActionResult GetRoleById(Guid id)
    {
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
    [RequirePermission("roles.create")]
    public IActionResult CreateRole([FromBody] Role role)
    {
        if (_roleRepository.GetRoleByName(role.Name) != null)
        {
            return BadRequest("Role name already exists");
        }
        
        var createdRole = _roleRepository.CreateRole(role);
        return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
    }
    
    [HttpPut("{id}")]
    [RequirePermission("roles.edit")]
    public IActionResult UpdateRole(Guid id, [FromBody] Role updatedRole)
    {
        var result = _roleRepository.UpdateRole(id, updatedRole);
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    [RequirePermission("roles.delete")]
    public IActionResult DeleteRole(Guid id)
    {
        var result = _roleRepository.DeleteRole(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
    [HttpGet("{id}/permissions")]
    [RequirePermission(new[] { "roles.view", "permissions.view" }, false)]
    public IActionResult GetRolePermissions(Guid id)
    {
        var role = _roleRepository.GetRoleById(id);
        if (role == null)
        {
            return NotFound();
        }
        
        var permissions = _permissionRepository.GetRolePermissions(id);
        return Ok(permissions);
    }
    
    [HttpPost("{roleId}/permissions/{permissionId}")]
    [RequirePermission(new[] { "roles.edit", "permissions.assign" }, false)]
    public IActionResult AddPermissionToRole(Guid roleId, Guid permissionId)
    {
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
    [RequirePermission(new[] { "roles.edit", "permissions.assign" }, false)]
    public IActionResult RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
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