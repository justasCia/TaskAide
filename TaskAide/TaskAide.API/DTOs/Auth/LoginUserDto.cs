using System.ComponentModel.DataAnnotations;

namespace TaskAide.API.DTOs.Auth
{
    public class LoginUserDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = default!;
        [Required]
        public string Password { get; set; } = default!;
    }
}