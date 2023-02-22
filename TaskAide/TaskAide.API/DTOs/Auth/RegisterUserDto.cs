using System.ComponentModel.DataAnnotations;

namespace TaskAide.API.DTOs.Auth
{
    public class RegisterUserDto
    {
        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string PhoneNumber { get; set; } = default!;


        [Required]
        public string Password { get; set; } = default!;

        //[Required]
        //public string Role { get; set; } = default!;
    }
}
