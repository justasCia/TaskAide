using TaskAide.API.DTOs.Auth;

namespace TaskAide.API.Services.Auth
{
    public interface IAuthService
    {
        public Task<UserDto> RegisterUserAsync(RegisterUserDto registerUser);

        public Task<TokenDto> LoginUserAsync(LoginUserDto loginUser);

        public Task<TokenDto> RefreshToken(TokenDto tokenDto);
    }
}
