using System.Security.Claims;

namespace TaskAide.API.Services.Auth
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(string email, string userId, IEnumerable<string> userRoles);
        string GenerateRefreshToken();
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
    }
}
