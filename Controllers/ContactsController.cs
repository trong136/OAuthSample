using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthSample.Models;
using OAuthSample.Services;

namespace OAuthSample.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class ContactsController : ControllerBase
{
    private readonly ContactRepository _contactRepository;

    public ContactsController(ContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    // GET: api/contacts
    [HttpGet]
    public IActionResult GetContacts()
    {
        // Get the current user's username
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        var contacts = _contactRepository.GetAllContacts(username);
        return Ok(contacts);
    }

    // GET: api/contacts/{id}
    [HttpGet("{id}")]
    public IActionResult GetContact(Guid id)
    {
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        var contact = _contactRepository.GetContactById(id, username);
        
        if (contact == null)
        {
            return NotFound(new { message = "Contact not found" });
        }
        
        return Ok(contact);
    }

    // POST: api/contacts
    [HttpPost]
    public IActionResult CreateContact([FromBody] Contact contact)
    {
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        // Set the owner to the current user
        contact.OwnerUsername = username;
        
        var newContact = _contactRepository.AddContact(contact);
        return CreatedAtAction(nameof(GetContact), new { id = newContact.Id }, newContact);
    }

    // PUT: api/contacts/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateContact(Guid id, [FromBody] Contact contact)
    {
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (id != contact.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }
        
        // Ensure the contact belongs to the current user
        contact.OwnerUsername = username;
        
        var success = _contactRepository.UpdateContact(contact, username);
        
        if (!success)
        {
            return NotFound(new { message = "Contact not found" });
        }
        
        return NoContent();
    }

    // DELETE: api/contacts/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteContact(Guid id)
    {
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        var success = _contactRepository.DeleteContact(id, username);
        
        if (!success)
        {
            return NotFound(new { message = "Contact not found" });
        }
        
        return NoContent();
    }
} 