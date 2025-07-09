using System.ComponentModel.DataAnnotations;

namespace Tournament.Shared.DTOs;
public record UserRegistrationDto
{
    [Required(ErrorMessage = "User Name is required field")]
    public required string Name { get; init; }

    [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
    public int Age { get; init; }
    public string? Position { get; init; }

    [Required(ErrorMessage = "Email is required field")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required field")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public required string Password { get; init; }
}
