using TaskAide.API.DTOs.Auth;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.API.Services.Auth
{
    public interface IAuthService
    {
        public Task<User> RegisterUserAsync(RegisterUserDto registerUser, RegisterType registerType);
        public Task<UserDto> RegisterCompanyAsync(RegisterCompanyDto registerCompany);

        public Task<TokenDto> LoginUserAsync(LoginUserDto loginUser);

        public Task<TokenDto> RefreshTokenAsync(TokenDto tokenDto);

        public Task RevokeTokenAsync(string userId, string refreshToken);

        public Task RemoveUserAsync(string userId);
    }
    public enum RegisterType
    {
        User,
        Provider,
        CompanyWorker
    }
}
