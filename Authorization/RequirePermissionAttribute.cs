using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OAuthSample.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _permissionNames;
    private readonly bool _requireAll;

    public RequirePermissionAttribute(string permissionName)
    {
        _permissionNames = new[] { permissionName };
        _requireAll = false;
    }

    public RequirePermissionAttribute(string[] permissionNames, bool requireAll = false)
    {
        _permissionNames = permissionNames;
        _requireAll = requireAll;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        // If user is not authenticated, return 401
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var permissionService = context.HttpContext.RequestServices.GetService<PermissionService>();
        if (permissionService == null)
        {
            // If the permission service is not available, deny access
            context.Result = new ForbidResult();
            return;
        }
        
        bool hasPermission = _requireAll
            ? permissionService.HasAllPermissions(user, _permissionNames)
            : permissionService.HasAnyPermission(user, _permissionNames);
            
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
} 