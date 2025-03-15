using System.ComponentModel.DataAnnotations;

namespace OAuthSample.Models;

/// <summary>
/// Represents a contact in the contact management system
/// </summary>
public class Contact
{
    /// <summary>
    /// Unique identifier for the contact
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Full name of the contact
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the contact
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number of the contact
    /// </summary>
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// Contact's company
    /// </summary>
    [StringLength(100)]
    public string? Company { get; set; }
    
    /// <summary>
    /// Contact's job title
    /// </summary>
    [StringLength(100)]
    public string? JobTitle { get; set; }
    
    /// <summary>
    /// Notes about the contact
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// When the contact was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the contact was last updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }
    
    /// <summary>
    /// Username of the user who owns this contact
    /// </summary>
    [Required]
    public string OwnerUsername { get; set; } = string.Empty;
} 