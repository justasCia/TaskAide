using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskAide.API.DTOs.Auth;
using TaskAide.API.Services.Auth;

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
            return Ok(await _authService.RegisterUserAsync(registerUserDto));
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
                Secure = false,
                Expires = new DateTimeOffset(token.RefreshTokenExpiryDate)
            };
            Response.Cookies.Append("refreshToken", token.RefreshToken, cookieOptions);
            return Ok(new { accessToken = token.AccessToken });
        }

        //[HttpPost]
        //[Route("signin-google")]
        //public async Task<IActionResult> SigninGoogle(LoginUserDto loginUserDto)
        //{
        //}

        [HttpPost]
        [Route("refreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
            {
                return Unauthorized();
            }

            var tokenDto = new TokenDto(refreshTokenDto.AccessToken, refreshToken, DateTime.Now);
            var token = await _authService.RefreshToken(tokenDto);
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = false,
                Expires = new DateTimeOffset(token.RefreshTokenExpiryDate)
            };
            Response.Cookies.Append("refreshToken", token.RefreshToken, cookieOptions);
            return Ok(new { accessToken = token.AccessToken });
        }
    }
}
