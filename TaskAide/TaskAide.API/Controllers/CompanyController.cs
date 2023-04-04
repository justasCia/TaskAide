using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TaskAide.API.DTOs.Auth;
using TaskAide.API.Services.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Services;

namespace TaskAide.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Company)]
    public class CompanyController : ControllerBase
    {
        private readonly IProviderService _providerService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public CompanyController(IProviderService providerService, IAuthService authService, IMapper mapper)
        {
            _providerService = providerService;
            _authService = authService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetCompanyWorkers()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var workers = await _providerService.GetCompanyWorkersAsync(userId!);

            return Ok(workers.Select(w => _mapper.Map<UserDto>(w)));
        }

        [HttpPost]
        [Route("users")]
        public async Task<IActionResult> RegisterCompanyWorker(RegisterUserDto registerUserDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var user = await _authService.RegisterUserAsync(registerUserDto, RegisterType.CompanyWorker);
            try
            {
                var newCompanyWorker = await _providerService.AddCompanyWorkerAsync(userId!, user);
                return Ok(_mapper.Map<UserDto>(newCompanyWorker));
            }
            catch
            {
                await _authService.RemoveUserAsync(user.Id);
                throw;
            }
        }
    }
}
