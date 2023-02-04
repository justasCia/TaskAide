using TaskAide.API.DTOs.Auth;

namespace TaskAide.API.Services.Auth
{
    public interface IAuthService
    {
        public Task<UserDto> RegisterUserAsync(RegisterUserDto registerUser);

        public Task<string> LoginUserAsync(LoginUserDto loginUser);
    }
}
