namespace OAuthSample.Models;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // E.g., "Users", "Contacts", etc.
    public string Action { get; set; } = string.Empty; // E.g., "Create", "Read", "Update", "Delete"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
} 