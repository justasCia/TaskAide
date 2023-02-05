using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TaskAide.Domain.Entities.Auth;

namespace TaskAide.Domain.Entities.Users
{
    public class User : IdentityUser
    {
        //public string? RefreshToken { get; set; }
        //public DateTime RefreshTokenExpiryTime { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = default!;
    }
}
