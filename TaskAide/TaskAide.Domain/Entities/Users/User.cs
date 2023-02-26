using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Bookings;

namespace TaskAide.Domain.Entities.Users
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public bool IsProvider { get; set; } = default!;
        public Provider Provider { get; set; } = default!;
        public ICollection<RefreshToken> RefreshTokens { get; set; } = default!;
    }
}
