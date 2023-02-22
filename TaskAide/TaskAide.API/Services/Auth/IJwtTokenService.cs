using System.Security.Claims;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.API.Services.Auth
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, IEnumerable<string> userRoles);
        string GenerateRefreshToken();
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
    }
}
