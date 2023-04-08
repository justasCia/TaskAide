using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TaskAide.API.DTOs.Services;
using TaskAide.API.DTOs.Users;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Services;

namespace TaskAide.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService _providersService;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;

        public ProviderController(IProviderService providersService, IPaymentService paymentService, IMapper mapper)
        {
            _providersService = providersService;
            _paymentService = paymentService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("information")]
        public async Task<IActionResult> GetProviderInformation()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var provider = await _providersService.GetProviderAsync(userId!);

            return provider != null ? Ok(_mapper.Map<ProviderWithInformationDto>(provider)) : NotFound();
        }

        [HttpPut]
        [Route("information")]
        public async Task<IActionResult> UpsertProviderInformation(ProviderInformationDto providerDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var provider = await _providersService.UpsertProviderAsync(userId!, _mapper.Map<Provider>(providerDto));
            var services = await _providersService.PostProviderServicesAsync(userId!, providerDto.ProviderServices.Select(ps => ps.Id));

            var providerResponse = _mapper.Map<ProviderWithInformationDto>(provider);
            providerResponse.ProviderServices = services.Select(s => new ServiceDto() { Id = s.Id, Name = s.Name });

            return Ok(providerResponse);
        }

        [HttpPost]
        [Route("bankAccount")]
        public async Task<IActionResult> AddProviderBankAccount([FromBody] string bankAccountNumber)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var provider = await _providersService.GetProviderAsync(userId!);

            if (provider == null)
            {
                throw new NotFoundException("Provider not found");
            }

            provider = await _paymentService.AddBankAccountAsync(provider, bankAccountNumber, HttpContext.Connection.RemoteIpAddress!.ToString());

            return Ok(_mapper.Map<ProviderDto>(provider));
        }

        [HttpPut]
        [Route("bankAccount")]
        public async Task<IActionResult> UpdateProviderBankAccount([FromBody] string bankAccountNumber)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var provider = await _providersService.GetProviderAsync(userId!);

            if (provider == null)
            {
                throw new NotFoundException("Provider not found");
            }

            provider = await _paymentService.UpdateBankAccountAsync(provider, bankAccountNumber);

            return Ok(_mapper.Map<ProviderDto>(provider));
        }

        [HttpGet]
        [Route("report")]
        public async Task<IActionResult> GetProviderReport()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Ok(await _providersService.GetProviderReportAsync(userId!));
        }
    }
}
