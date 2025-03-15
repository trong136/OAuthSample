using System.Security.Claims;
using OAuthSample.Models;
using OAuthSample.Services;

namespace OAuthSample.Authorization;

public class PermissionService
{
    private readonly RoleRepository _roleRepository;
    private readonly PermissionRepository _permissionRepository;
    private readonly UserRepository _userRepository;

    public PermissionService(
        RoleRepository roleRepository,
        PermissionRepository permissionRepository,
        UserRepository userRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _userRepository = userRepository;
    }

    public bool HasPermission(ClaimsPrincipal user, string permissionName)
    {
        // If the user is not authenticated, they have no permissions
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        // Get the user's ID from the claims
        var userIdClaim = user.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return false;
        }

        // Get the user
        var dbUser = _userRepository.GetUserById(userId);
        if (dbUser == null || !dbUser.IsActive)
        {
            return false;
        }

        // Get the user's roles
        var roles = _roleRepository.GetUserRoles(userId);
        if (!roles.Any())
        {
            return false;
        }

        // Check if any of the user's roles have the required permission
        foreach (var role in roles)
        {
            if (!role.IsActive)
            {
                continue;
            }

            var permissions = _permissionRepository.GetRolePermissions(role.Id);
            if (permissions.Any(p => p.IsActive && p.Name == permissionName))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAnyPermission(ClaimsPrincipal user, params string[] permissionNames)
    {
        return permissionNames.Any(permissionName => HasPermission(user, permissionName));
    }

    public bool HasAllPermissions(ClaimsPrincipal user, params string[] permissionNames)
    {
        return permissionNames.All(permissionName => HasPermission(user, permissionName));
    }

    public List<Permission> GetUserPermissions(Guid userId)
    {
        var permissions = new List<Permission>();
        
        // Get the user's roles
        var roles = _roleRepository.GetUserRoles(userId);
        if (!roles.Any())
        {
            return permissions;
        }

        // Get all permissions for each role
        foreach (var role in roles)
        {
            if (!role.IsActive)
            {
                continue;
            }

            var rolePermissions = _permissionRepository.GetRolePermissions(role.Id)
                .Where(p => p.IsActive)
                .ToList();
                
            // Add permissions not already in the list
            foreach (var permission in rolePermissions)
            {
                if (!permissions.Any(p => p.Id == permission.Id))
                {
                    permissions.Add(permission);
                }
            }
        }

        return permissions;
    }
} 