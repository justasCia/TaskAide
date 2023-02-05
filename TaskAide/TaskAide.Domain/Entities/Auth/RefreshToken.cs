using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Entities.Auth
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = default!;
        public DateTime RefreshTokenExpiryTime { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
