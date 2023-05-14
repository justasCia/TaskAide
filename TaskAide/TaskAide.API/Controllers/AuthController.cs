using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using TaskAide.API.DTOs.Auth;
using TaskAide.API.Services.Auth;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace TaskAide.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
        {
            var registeredUser = await _authService.RegisterUserAsync(registerUserDto,RegisterType.User);
            return Ok(new UserDto() { Email = registeredUser.Email });
        }

        [HttpPost]
        [Route("registerProvider")]
        public async Task<IActionResult> RegisterProvider(RegisterUserDto registerUserDto)
        {
            var regosteredProvider = await _authService.RegisterUserAsync(registerUserDto, RegisterType.Provider);
            return Ok(new UserDto() { Email = regosteredProvider.Email });
        }

        [HttpPost]
        [Route("registerCompany")]
        public async Task<IActionResult> RegisterCompany(RegisterCompanyDto registerCompanyDto)
        {
            return Ok(await _authService.RegisterCompanyAsync(registerCompanyDto));
        }


        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Signin(LoginUserDto loginUserDto)
        {
            var token = await _authService.LoginUserAsync(loginUserDto);
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                Expires = new DateTimeOffset(token.RefreshTokenExpiryDate),
                SameSite = SameSiteMode.None
            };
            Response.Cookies.Append("refreshToken", token.RefreshToken, cookieOptions);
            return Ok(new { accessToken = token.AccessToken });
        }

        [HttpPost]
        [Route("refreshToken")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].FirstOrDefault();
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null || accessToken == null)
            {
                return Unauthorized();
            }
            accessToken = accessToken.Replace("Bearer ", "");

            var tokenDto = new TokenDto(accessToken, refreshToken, DateTime.Now);
            var token = await _authService.RefreshTokenAsync(tokenDto);
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                Expires = new DateTimeOffset(token.RefreshTokenExpiryDate),
                SameSite = SameSiteMode.None
            };
            Response.Cookies.Append("refreshToken", token.RefreshToken, cookieOptions);
            return Ok(new { accessToken = token.AccessToken });
        }

        [HttpPost]
        [Route("revokeToken")]
        [Authorize]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var refreshToken = Request.Cookies["refreshToken"];


            if (refreshToken == null || userId == null)
            {
                return Unauthorized();
            }

            await _authService.RevokeTokenAsync(userId, refreshToken);

            Response.Cookies.Delete("refreshToken");

            return NoContent();
        }
    }
}
