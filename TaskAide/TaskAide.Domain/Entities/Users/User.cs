using Microsoft.AspNetCore.Identity;
using TaskAide.Domain.Entities.Auth;

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
