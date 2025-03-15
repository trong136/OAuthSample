using OAuthSample.Models;

namespace OAuthSample.Services;

public class UserRepository
{
    private readonly List<User> _users = new();

    public UserRepository()
    {
        // Add a default admin user
        var adminUser = new User
        {
            Username = "admin",
            Password = "Admin123!",  // In a real application, this would be hashed
            Email = "admin@example.com",
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true
        };
        _users.Add(adminUser);
        
        // Add a regular user
        var regularUser = new User
        {
            Username = "user",
            Password = "User123!",  // In a real application, this would be hashed
            Email = "user@example.com",
            FirstName = "Regular",
            LastName = "User",
            IsActive = true
        };
        _users.Add(regularUser);
        
        Console.WriteLine("Default admin user created:");
        Console.WriteLine("Username: admin");
        Console.WriteLine("Password: Admin123!");
        
        Console.WriteLine("\nDefault regular user created:");
        Console.WriteLine("Username: user");
        Console.WriteLine("Password: User123!");
    }

    public List<User> GetAllUsers()
    {
        return _users;
    }

    public User? GetUserById(Guid id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public User? GetUserByUsername(string username)
    {
        return _users.FirstOrDefault(u => u.Username == username);
    }

    public User CreateUser(User user)
    {
        _users.Add(user);
        return user;
    }

    public User? UpdateUser(Guid id, User updatedUser)
    {
        var existingUser = GetUserById(id);
        if (existingUser == null) return null;

        existingUser.Username = updatedUser.Username;
        existingUser.Email = updatedUser.Email;
        existingUser.FirstName = updatedUser.FirstName;
        existingUser.LastName = updatedUser.LastName;
        existingUser.IsActive = updatedUser.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;
        
        // Don't update the password here, use a separate method
        return existingUser;
    }

    public bool DeleteUser(Guid id)
    {
        var user = GetUserById(id);
        if (user == null) return false;
        
        return _users.Remove(user);
    }

    public User? UpdateUserPassword(Guid id, string newPassword)
    {
        var user = GetUserById(id);
        if (user == null) return null;
        
        user.Password = newPassword; // In a real application, this would be hashed
        user.UpdatedAt = DateTime.UtcNow;
        return user;
    }
} 