using OAuthSample.Models;

namespace OAuthSample.Services;

/// <summary>
/// Repository for managing contacts
/// This service is registered as a singleton to persist contacts across requests
/// </summary>
public class ContactRepository
{
    // In-memory store for contacts (simulating a database)
    private readonly List<Contact> _contacts = new();

    public ContactRepository()
    {
        // Add some sample contacts for demo purposes
        _contacts.Add(new Contact
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Company = "Acme Inc.",
            Notes = "Met at conference",
            OwnerUsername = "admin"
        });
        
        _contacts.Add(new Contact
        {
            Name = "Jane Smith",
            Email = "jane.smith@example.com",
            Phone = "555-123-4567",
            Company = "TechCorp",
            Notes = "Potential client",
            OwnerUsername = "admin"
        });
        
        Console.WriteLine("ContactRepository initialized with sample data");
    }

    /// <summary>
    /// Get all contacts for a specific user
    /// </summary>
    public IEnumerable<Contact> GetAllContacts(string username)
    {
        return _contacts.Where(c => c.OwnerUsername == username).ToList();
    }

    /// <summary>
    /// Get a contact by ID
    /// </summary>
    public Contact? GetContactById(Guid id, string username)
    {
        return _contacts.FirstOrDefault(c => c.Id == id && c.OwnerUsername == username);
    }

    /// <summary>
    /// Add a new contact
    /// </summary>
    public Contact AddContact(Contact contact)
    {
        contact.CreatedDate = DateTime.UtcNow;
        _contacts.Add(contact);
        Console.WriteLine($"Added contact: {contact.Name} for user {contact.OwnerUsername}");
        return contact;
    }

    /// <summary>
    /// Update an existing contact
    /// </summary>
    public bool UpdateContact(Contact contact, string username)
    {
        var existingContact = _contacts.FirstOrDefault(c => c.Id == contact.Id && c.OwnerUsername == username);
        
        if (existingContact == null)
            return false;
            
        // Update properties
        existingContact.Name = contact.Name;
        existingContact.Email = contact.Email;
        existingContact.Phone = contact.Phone;
        existingContact.Company = contact.Company;
        existingContact.Notes = contact.Notes;
        existingContact.LastUpdated = DateTime.UtcNow;
        
        Console.WriteLine($"Updated contact: {contact.Name} for user {contact.OwnerUsername}");
        return true;
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    public bool DeleteContact(Guid id, string username)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id && c.OwnerUsername == username);
        
        if (contact == null)
            return false;
            
        _contacts.Remove(contact);
        Console.WriteLine($"Deleted contact: {contact.Name} for user {contact.OwnerUsername}");
        return true;
    }
} 