﻿using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TaskAide.API.Common;
using TaskAide.API.DTOs.Auth;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;
using TaskAide.Domain.Services;

namespace TaskAide.API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefrehTokenRepository _refreshTokenRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly int _refreshTokenValidityInDays;

        public AuthService(UserManager<User> userManager, IJwtTokenService jwtTokenService, IRefrehTokenRepository refreshTokenRepository, IEncryptionService encryptionService, IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _encryptionService = encryptionService;
            _refreshTokenValidityInDays = int.Parse(configuration[Constants.Configuration.Jwt.RefreshTokenValidityInDays] ?? "1");
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto registerUser, RegisterType registerType)
        {
            var user = await _userManager.FindByEmailAsync(registerUser.Email);

            if (user != null)
            {
                throw new BadRequestException("Email already taken.");
            }

            var newUser = new User
            {
                FirstName = registerUser.FirstName,
                LastName = registerUser.LastName,
                PhoneNumber = registerUser.PhoneNumber,
                Email = registerUser.Email,
                UserName = registerUser.Email,
                IsProvider = registerType == RegisterType.Provider || registerType == RegisterType.CompanyWorker 
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerUser.Password);
            if (!createUserResult.Succeeded)
            {
                throw new BadRequestException("Could not create a user.");
            }

            switch (registerType)
            {
                case RegisterType.User:
                    await _userManager.AddToRoleAsync(newUser, Roles.Client);
                    break;
                case RegisterType.Provider:
                    await _userManager.AddToRoleAsync(newUser, Roles.Provider);
                    break;
                case RegisterType.CompanyWorker:
                    await _userManager.AddToRoleAsync(newUser, Roles.CompanyWorker);
                    break;
            }

            return newUser;
        }

        public async Task<UserDto> RegisterCompanyAsync(RegisterCompanyDto registerCompany)
        {
            var user = await _userManager.FindByEmailAsync(registerCompany.Email);

            if (user != null)
            {
                throw new BadRequestException("Email already taken.");
            }

            var newUser = new User
            {
                PhoneNumber = registerCompany.PhoneNumber,
                Email = registerCompany.Email,
                UserName = registerCompany.Email,
                CompanyName = registerCompany.CompanyName,
                IsProvider = true
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerCompany.Password);

            if (!createUserResult.Succeeded)
            {
                throw new BadRequestException("Could not create a user.");
            }

            await _userManager.AddToRoleAsync(newUser, Roles.Company);

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

            return await GenerateTokenAsync(user);
        }



        public async Task<TokenDto> RefreshTokenAsync(TokenDto tokenDto)
        {
            string accessToken = tokenDto.AccessToken;
            string refreshToken = tokenDto.RefreshToken;

            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken);
            var email = principal.FindFirstValue("email");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid access token or refresh token");
            }

            var dbRefreshTokens = await _refreshTokenRepository.ListAsync(rt => rt.UserId == user.Id);
            var dbRefreshToken = dbRefreshTokens.FirstOrDefault(rt => _encryptionService.DecryptString(rt.Token) == refreshToken);

            if (dbRefreshToken == null || dbRefreshToken.RefreshTokenExpiryTime < DateTime.Now)
            {
                throw new UnauthorizedException("Invalid access token or refresh token");
            }

            await _refreshTokenRepository.DeleteAsync(dbRefreshToken);

            return await GenerateTokenAsync(user);
        }

        public async Task RevokeTokenAsync(string userId, string refreshToken)
        {
            var refreshTokenDb = await _refreshTokenRepository.GetAsync(rt => rt.UserId == userId && rt.Token == refreshToken);

            if (refreshTokenDb != null)
            {
                await _refreshTokenRepository.DeleteAsync(refreshTokenDb);
            }
        }

        private async Task<TokenDto> GenerateTokenAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken()
            {
                Token = _encryptionService.EncryptString(refreshToken),
                RefreshTokenExpiryTime = DateTime.Now.AddDays(_refreshTokenValidityInDays),
                UserId = user.Id
            });

            return new TokenDto(accessToken, refreshToken, DateTime.Now.AddDays(_refreshTokenValidityInDays));
        }

        public async Task RemoveUserAsync(string userId)
        {
            await _userManager.DeleteAsync(await _userManager.FindByIdAsync(userId));
        }
    }
}
