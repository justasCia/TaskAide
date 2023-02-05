using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskAide.API.DTOs.Auth;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;

namespace TaskAide.API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly int _refreshTokenValidityInDays;
        private readonly IRefrehTokenRepository _refreshTokenRepository;

        public AuthService(UserManager<User> userManager, IJwtTokenService jwtTokenService, IRefrehTokenRepository refreshTokenRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenValidityInDays = int.Parse(configuration["JWT:RefreshTokenValidityInDays"] ?? "1");
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

        public async Task<TokenDto> LoginUserAsync(LoginUserDto loginUser)
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

            var expiredUserRefreshTokens = await _refreshTokenRepository.ListAsync(rt => rt.UserId == user.Id && rt.RefreshTokenExpiryTime < DateTime.Now);
            await _refreshTokenRepository.DeleteListAsync(expiredUserRefreshTokens);

            return await GenerateToken(user);
        }

        

        public async Task<TokenDto> RefreshToken(TokenDto tokenDto)
        {
            string accessToken = tokenDto.AccessToken;
            string refreshToken = tokenDto.RefreshToken;

            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new BadRequestException("Invalid access token or refresh token");
            }

            var dbRefreshToken = await _refreshTokenRepository.GetAsync(rt => rt.Token == refreshToken && rt.UserId == user.Id);

            if (dbRefreshToken == null || dbRefreshToken.RefreshTokenExpiryTime < DateTime.Now)
            {
                throw new BadRequestException("Invalid access token or refresh token");
            }

            await _refreshTokenRepository.DeleteAsync(dbRefreshToken);

            return await GenerateToken(user);
        }

        private async Task<TokenDto> GenerateToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtTokenService.GenerateAccessToken(user.Email, user.Id, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken() { Token = refreshToken, RefreshTokenExpiryTime = DateTime.Now.AddDays(_refreshTokenValidityInDays), UserId = user.Id });

            return new TokenDto(accessToken, refreshToken);
        }
    }
}
