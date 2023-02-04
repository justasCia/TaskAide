using Microsoft.AspNetCore.Identity;
using TaskAide.API.DTOs.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;

namespace TaskAide.API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(UserManager<User> userManager, IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<UserDto> RegisterUserAsync(RegisterUserDto registerUser)
        {
            if (!Roles.All.Contains(registerUser.Role) || registerUser.Role == Roles.Admin)
            {
                throw new BadRequestException($"Could not create user with {registerUser.Role} role.");
            }

            var user = await _userManager.FindByEmailAsync(registerUser.Email);

            if (user != null)
            {
                throw new BadRequestException("Email already taken.");
            }

            var newUser = new User
            {
                Email = registerUser.Email,
                UserName = registerUser.Email
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerUser.Password);
            if (!createUserResult.Succeeded)
            {
                throw new BadRequestException("Could not create a user.");
            }

            await _userManager.AddToRoleAsync(newUser, registerUser.Role);

            return new UserDto() { Email = newUser.Email };
        }

        public async Task<string> LoginUserAsync(LoginUserDto loginUser)
        {
            var user = await _userManager.FindByEmailAsync(loginUser.Email);

            if (user == null)
            {
                throw new NotFoundException("User with such email or password not found.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginUser.Password);

            if (!isPasswordValid)
            {
                throw new NotFoundException("User with such email or password not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.CreateAccessToken(user.Email, user.Id, roles);

            return accessToken;
        }
    }
}
