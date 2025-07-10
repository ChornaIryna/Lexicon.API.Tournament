using System.ComponentModel.DataAnnotations;

namespace Tournament.Shared.DTOs;
public record UserLoginDto([Required] string UserName, [Required] string Password);
