using System.ComponentModel.DataAnnotations;

namespace TaskAide.API.DTOs.Auth
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
        [Required]
        public string Password { get; set; } = default!;
        [Required]
        public string Role { get; set; } = default!;
    }
}
