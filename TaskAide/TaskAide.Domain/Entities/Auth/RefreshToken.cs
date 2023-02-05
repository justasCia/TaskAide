using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Entities.Auth
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = default!;
        public DateTime RefreshTokenExpiryTime { get; set; }
        [Required]
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}
