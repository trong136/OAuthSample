using OAuthSample.Models;

namespace OAuthSample.Services;

public class RoleRepository
{
    private readonly List<Role> _roles = new();
    private readonly List<UserRole> _userRoles = new();

    public RoleRepository()
    {
        // Add default roles
        var adminRole = new Role
        {
            Name = "Administrator",
            Description = "Full system access"
        };
        
        var userRole = new Role
        {
            Name = "User",
            Description = "Standard user access"
        };
        
        _roles.Add(adminRole);
        _roles.Add(userRole);
    }

    public List<Role> GetAllRoles()
    {
        return _roles;
    }

    public Role? GetRoleById(Guid id)
    {
        return _roles.FirstOrDefault(r => r.Id == id);
    }

    public Role? GetRoleByName(string name)
    {
        return _roles.FirstOrDefault(r => r.Name == name);
    }

    public Role CreateRole(Role role)
    {
        _roles.Add(role);
        return role;
    }

    public Role? UpdateRole(Guid id, Role updatedRole)
    {
        var existingRole = GetRoleById(id);
        if (existingRole == null) return null;

        existingRole.Name = updatedRole.Name;
        existingRole.Description = updatedRole.Description;
        existingRole.IsActive = updatedRole.IsActive;
        existingRole.UpdatedAt = DateTime.UtcNow;
        
        return existingRole;
    }

    public bool DeleteRole(Guid id)
    {
        var role = GetRoleById(id);
        if (role == null) return false;
        
        // Remove all user-role associations for this role
        _userRoles.RemoveAll(ur => ur.RoleId == id);
        
        return _roles.Remove(role);
    }

    // User-Role assignment methods
    public UserRole AssignRoleToUser(Guid userId, Guid roleId)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
        
        _userRoles.Add(userRole);
        return userRole;
    }

    public bool RemoveRoleFromUser(Guid userId, Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole == null) return false;
        
        return _userRoles.Remove(userRole);
    }

    public List<Role> GetUserRoles(Guid userId)
    {
        var roleIds = _userRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToList();
        
        return _roles.Where(r => roleIds.Contains(r.Id)).ToList();
    }

    public List<UserRole> GetAllUserRoles()
    {
        return _userRoles;
    }
} 