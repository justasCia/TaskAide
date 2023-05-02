using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskAide.API.Controllers;
using TaskAide.API.DTOs;
using TaskAide.API.DTOs.Users;
using TaskAide.Domain.Entities.Reports;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Services;

namespace TaskAide.UnitTests.ControllersTests
{
    public class ProviderControllerTests
    {
        private Mock<IProviderService> _mockProviderService;
        private Mock<IPaymentService> _mockPaymentService;

        private ProviderController _providerController;

        [SetUp]
        public void SetUp()
        {
            _mockProviderService = new Mock<IProviderService>();
            _mockPaymentService = new Mock<IPaymentService>();
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            _providerController = new ProviderController(_mockProviderService.Object, _mockPaymentService.Object, mapper);

            _providerController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, "testUserId")
                    }, "mock")),
                }
            };
            _providerController.ControllerContext.HttpContext.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[4]);
        }

        [Test]
        public async Task GetProviderInformation_FindsProvider_ReturnsOkObjectResult()
        {
            _mockProviderService.Setup(m => m.GetProviderAsync(It.IsAny<string>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());

            var result = await _providerController.GetProviderInformation() as OkObjectResult;
            var resultValue = result!.Value as ProviderDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task UpsertProviderInformation_Successful_ReturnsOkObjectResult()
        {
            _mockProviderService.Setup(m => m.UpsertProviderAsync(It.IsAny<string>(), It.IsAny<Provider>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());
            _mockProviderService.Setup(m => m.PostProviderServicesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(Builder<Service>.CreateListOfSize(2).Build());

            var result = await _providerController.UpsertProviderInformation(Builder<ProviderInformationDto>.CreateNew().Build()) as OkObjectResult;
            var resultValue = result!.Value as ProviderWithInformationDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task AddProviderBankAccount_Successful_ReturnsOkObjectResult()
        {
            _mockProviderService.Setup(m => m.GetProviderAsync(It.IsAny<string>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());
            _mockPaymentService.Setup(m => m.AddBankAccountAsync(It.IsAny<Provider>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());

            var result = await _providerController.AddProviderBankAccount("abc") as OkObjectResult;
            var resultValue = result!.Value as ProviderDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task AddProviderBankAccount_ProviderNotFound_ThrowsException()
        {
            var act = async () => await _providerController.AddProviderBankAccount("abc");

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task UpdateProviderBankAccount_Successful_ReturnsOkObjectResult()
        {
            _mockProviderService.Setup(m => m.GetProviderAsync(It.IsAny<string>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());
            _mockPaymentService.Setup(m => m.UpdateBankAccountAsync(It.IsAny<Provider>(), It.IsAny<string>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());

            var result = await _providerController.UpdateProviderBankAccount("abc") as OkObjectResult;
            var resultValue = result!.Value as ProviderDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task UpdateProviderBankAccount_ProviderNotFound_ThrowsException()
        {
            var act = async () => await _providerController.UpdateProviderBankAccount("abc");

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task GetProviderReport_Success_ReturnsOkObjectResult()
        {
            _mockProviderService.Setup(m => m.GetProviderReportAsync(It.IsAny<string>(), null, null)).ReturnsAsync(Builder<ProviderReport>.CreateNew().Build());

            var result = await _providerController.GetProviderReport() as OkObjectResult;
            var resultValue = result!.Value;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task GetWorkerReport_Success_ReturnsOkObjectResult()
        {
            _mockProviderService.Setup(m => m.GetWorkerReportAsync(It.IsAny<string>(), null, null)).ReturnsAsync(Builder<WorkerReport>.CreateNew().Build());

            var result = await _providerController.GetWorkerReport() as OkObjectResult;
            var resultValue = result!.Value;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }
    }
}
