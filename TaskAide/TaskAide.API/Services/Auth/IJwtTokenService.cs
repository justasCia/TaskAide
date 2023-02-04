namespace TaskAide.API.Services.Auth
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(string email, string userId, IEnumerable<string> userRoles);
    }
}
