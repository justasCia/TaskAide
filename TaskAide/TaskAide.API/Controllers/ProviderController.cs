using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TaskAide.API.DTOs.Services;
using TaskAide.API.DTOs.Users;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Services;

namespace TaskAide.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase
    {
        private readonly IProvidersService _providersService;
        private readonly IMapper _mapper;

        public ProviderController(IProvidersService providersService, IMapper mapper)
        {
            _providersService = providersService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("information")]
        [Authorize]
        public async Task<IActionResult> GetProviderInfo()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var provider = await _providersService.GetProviderAsync(userId!);

            return provider != null ? Ok(_mapper.Map<ProviderDto>(provider)) : NotFound();
        }

        [HttpPost]
        [Route("information")]
        [Authorize]
        public async Task<IActionResult> PostProviderInfo(ProviderInformationDto providerDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var provider = await _providersService.PostProviderAsync(userId!, _mapper.Map<Provider>(providerDto));
            var services = await _providersService.PostProviderServicesAsync(userId!, providerDto.ProviderServices.Select(ps => ps.Id));

            var providerResponse = _mapper.Map<ProviderWithInformationDto>(provider);
            providerResponse.ProviderServices = services.Select(s => new ServiceDto() { Id = s.Id, Name = s.Name });

            return Ok(providerResponse);
        }
    }
}
