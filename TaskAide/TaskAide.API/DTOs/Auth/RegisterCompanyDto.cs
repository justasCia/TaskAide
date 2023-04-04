using System.ComponentModel.DataAnnotations;

namespace TaskAide.API.DTOs.Auth
{
    public class RegisterCompanyDto
    {
        [Required]
        public string CompanyName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string PhoneNumber { get; set; } = default!;


        [Required]
        public string Password { get; set; } = default!;
    }
}
