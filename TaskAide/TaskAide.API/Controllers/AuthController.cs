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
        [Route("login")]
        public async Task<IActionResult> Login(LoginUserDto loginUserDto)
        {
            return Ok(await _authService.LoginUserAsync(loginUserDto));
        }

        [HttpPost]
        [Route("refreshToken")]
        public async Task<IActionResult> RefreshToken(TokenDto tokenDto)
        {
            return Ok(await _authService.RefreshToken(tokenDto)); 
        }
    }
}
