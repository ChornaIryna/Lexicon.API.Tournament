using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Tournament.Core.Entities;
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "User Name is required field")]
    public required string Name { get; set; }

    [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
    public int Age { get; set; }
    public string? Position { get; set; }
}
