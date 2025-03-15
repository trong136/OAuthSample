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
    private readonly PermissionService _permissionService;
    
    public PermissionsController(
        PermissionRepository permissionRepository,
        PermissionService permissionService)
    {
        _permissionRepository = permissionRepository;
        _permissionService = permissionService;
    }
    
    [HttpGet]
    public IActionResult GetAllPermissions()
    {
        if (!_permissionService.HasPermission(User, "permissions.view"))
        {
            return Forbid();
        }
        
        var permissions = _permissionRepository.GetAllPermissions();
        return Ok(permissions);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetPermissionById(Guid id)
    {
        if (!_permissionService.HasPermission(User, "permissions.view"))
        {
            return Forbid();
        }
        
        var permission = _permissionRepository.GetPermissionById(id);
        if (permission == null)
        {
            return NotFound();
        }
        
        return Ok(permission);
    }
    
    [HttpPost]
    public IActionResult CreatePermission([FromBody] Permission permission)
    {
        if (!_permissionService.HasPermission(User, "permissions.assign"))
        {
            return Forbid();
        }
        
        if (_permissionRepository.GetPermissionByName(permission.Name) != null)
        {
            return BadRequest("Permission name already exists");
        }
        
        var createdPermission = _permissionRepository.CreateCustomPermission(permission);
        return CreatedAtAction(nameof(GetPermissionById), new { id = createdPermission.Id }, createdPermission);
    }
    
    [HttpPut("{id}")]
    public IActionResult UpdatePermission(Guid id, [FromBody] Permission updatedPermission)
    {
        if (!_permissionService.HasPermission(User, "permissions.assign"))
        {
            return Forbid();
        }
        
        var result = _permissionRepository.UpdatePermission(id, updatedPermission);
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public IActionResult DeletePermission(Guid id)
    {
        if (!_permissionService.HasPermission(User, "permissions.assign"))
        {
            return Forbid();
        }
        
        var result = _permissionRepository.DeletePermission(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
} 