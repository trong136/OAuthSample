using OAuthSample.Models;

namespace OAuthSample.Services;

public class PermissionRepository
{
    private readonly List<Permission> _permissions = new();
    private readonly List<RolePermission> _rolePermissions = new();

    public PermissionRepository()
    {
        // Add default permissions
        AddDefaultPermissions();
    }

    private void AddDefaultPermissions()
    {
        // User management permissions
        CreatePermission("users.view", "View users", "Users", "Read");
        CreatePermission("users.create", "Create users", "Users", "Create");
        CreatePermission("users.edit", "Edit users", "Users", "Update");
        CreatePermission("users.delete", "Delete users", "Users", "Delete");
        
        // Role management permissions
        CreatePermission("roles.view", "View roles", "Roles", "Read");
        CreatePermission("roles.create", "Create roles", "Roles", "Create");
        CreatePermission("roles.edit", "Edit roles", "Roles", "Update");
        CreatePermission("roles.delete", "Delete roles", "Roles", "Delete");
        
        // Permission management permissions
        CreatePermission("permissions.view", "View permissions", "Permissions", "Read");
        CreatePermission("permissions.assign", "Assign permissions", "Permissions", "Create");
        
        // Contact management permissions
        CreatePermission("contacts.view", "View contacts", "Contacts", "Read");
        CreatePermission("contacts.create", "Create contacts", "Contacts", "Create");
        CreatePermission("contacts.edit", "Edit contacts", "Contacts", "Update");
        CreatePermission("contacts.delete", "Delete contacts", "Contacts", "Delete");
    }

    private Permission CreatePermission(string name, string description, string resource, string action)
    {
        var permission = new Permission
        {
            Name = name,
            Description = description,
            Resource = resource,
            Action = action
        };
        
        _permissions.Add(permission);
        return permission;
    }

    public List<Permission> GetAllPermissions()
    {
        return _permissions;
    }

    public Permission? GetPermissionById(Guid id)
    {
        return _permissions.FirstOrDefault(p => p.Id == id);
    }

    public Permission? GetPermissionByName(string name)
    {
        return _permissions.FirstOrDefault(p => p.Name == name);
    }

    public Permission CreateCustomPermission(Permission permission)
    {
        _permissions.Add(permission);
        return permission;
    }

    public Permission? UpdatePermission(Guid id, Permission updatedPermission)
    {
        var existingPermission = GetPermissionById(id);
        if (existingPermission == null) return null;

        existingPermission.Name = updatedPermission.Name;
        existingPermission.Description = updatedPermission.Description;
        existingPermission.Resource = updatedPermission.Resource;
        existingPermission.Action = updatedPermission.Action;
        existingPermission.IsActive = updatedPermission.IsActive;
        existingPermission.UpdatedAt = DateTime.UtcNow;
        
        return existingPermission;
    }

    public bool DeletePermission(Guid id)
    {
        var permission = GetPermissionById(id);
        if (permission == null) return false;
        
        // Remove all role-permission associations for this permission
        _rolePermissions.RemoveAll(rp => rp.PermissionId == id);
        
        return _permissions.Remove(permission);
    }

    // Role-Permission assignment methods
    public RolePermission AssignPermissionToRole(Guid roleId, Guid permissionId)
    {
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
        
        _rolePermissions.Add(rolePermission);
        return rolePermission;
    }

    public bool RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        if (rolePermission == null) return false;
        
        return _rolePermissions.Remove(rolePermission);
    }

    public List<Permission> GetRolePermissions(Guid roleId)
    {
        var permissionIds = _rolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToList();
        
        return _permissions.Where(p => permissionIds.Contains(p.Id)).ToList();
    }

    public List<RolePermission> GetAllRolePermissions()
    {
        return _rolePermissions;
    }
} 