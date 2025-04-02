using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthSample.Authorization;
using OAuthSample.Models;
using OAuthSample.Services;

namespace OAuthSample.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly PermissionRepository _permissionRepository;
    
    public PermissionsController(
        PermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }
    
    [HttpGet]
    [RequirePermission("permissions.view")]
    public IActionResult GetAllPermissions()
    {
        var permissions = _permissionRepository.GetAllPermissions();
        return Ok(permissions);
    }
    
    [HttpGet("{id}")]
    [RequirePermission("permissions.view")]
    public IActionResult GetPermissionById(Guid id)
    {
        var permission = _permissionRepository.GetPermissionById(id);
        if (permission == null)
        {
            return NotFound();
        }
        
        return Ok(permission);
    }
    
    [HttpPost]
    [RequirePermission("permissions.assign")]
    public IActionResult CreatePermission([FromBody] Permission permission)
    {
        if (_permissionRepository.GetPermissionByName(permission.Name) != null)
        {
            return BadRequest("Permission name already exists");
        }
        
        var createdPermission = _permissionRepository.CreateCustomPermission(permission);
        return CreatedAtAction(nameof(GetPermissionById), new { id = createdPermission.Id }, createdPermission);
    }
    
    [HttpPut("{id}")]
    [RequirePermission("permissions.assign")]
    public IActionResult UpdatePermission(Guid id, [FromBody] Permission updatedPermission)
    {
        var result = _permissionRepository.UpdatePermission(id, updatedPermission);
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    [RequirePermission("permissions.assign")]
    public IActionResult DeletePermission(Guid id)
    {
        var result = _permissionRepository.DeletePermission(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
} 