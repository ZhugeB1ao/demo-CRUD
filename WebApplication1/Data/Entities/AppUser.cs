using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Data.Entities;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    
    public string? Address { get; set; }

    public string? Role { get; set; }
}
