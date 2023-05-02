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
using TaskAide.API.DTOs.Auth;
using TaskAide.API.Services.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Services;

namespace TaskAide.UnitTests.ControllersTests
{
    public class CompanyControllerTests
    {
        private Mock<IProviderService> _mockProviderService;
        private Mock<IAuthService> _mockAuthService;

        private CompanyController _companyController;

        [SetUp]
        public void SetUp()
        {
            _mockProviderService = new Mock<IProviderService>();
            _mockAuthService = new Mock<IAuthService>();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            _companyController = new CompanyController(_mockProviderService.Object, _mockAuthService.Object, mapper);

            _companyController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, "testUserId")
                    }, "mock"))
                }
            };
        }

        [Test]
        public async Task GetCompanyWorkers_FindsWorkers_ReturnsOkObjectResult()
        {
            _mockProviderService.Setup(m => m.GetCompanyWorkersAsync(It.IsAny<string>())).ReturnsAsync(Builder<Provider>.CreateListOfSize(2).Build());

            var result = await _companyController.GetCompanyWorkers() as OkObjectResult;
            var resultValue = result!.Value as IEnumerable<UserDto>;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull().And.NotBeEmpty();
        }

        [Test]
        public async Task GetCompanyWorkers_CantFindCompanyWorkers_ThrowsExcception()
        {
            _mockProviderService.Setup(m => m.GetCompanyWorkersAsync(It.IsAny<string>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _companyController.GetCompanyWorkers();

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task RegisterCompanyWorker_Success_ReturnsOkObjectResult()
        {
            _mockAuthService.Setup(m => m.RegisterUserAsync(It.IsAny<RegisterUserDto>(), It.IsAny<RegisterType>())).ReturnsAsync(Builder<User>.CreateNew().Build());
            _mockProviderService.Setup(m => m.AddCompanyWorkerAsync(It.IsAny<string>(), It.IsAny<User>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());

            var result = await _companyController.RegisterCompanyWorker(It.IsAny<RegisterUserDto>()) as OkObjectResult;
            var resultValue = result!.Value as UserDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task RegisterCompanyWorker_FailsToAddCompanyWorker_RemovesUserAndThrowsException()
        {
            _mockAuthService.Setup(m => m.RegisterUserAsync(It.IsAny<RegisterUserDto>(), It.IsAny<RegisterType>())).ReturnsAsync(Builder<User>.CreateNew().Build());
            _mockProviderService.Setup(m => m.AddCompanyWorkerAsync(It.IsAny<string>(), It.IsAny<User>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _companyController.RegisterCompanyWorker(It.IsAny<RegisterUserDto>());

            await act.Should().ThrowAsync<NotFoundException>();
            _mockAuthService.Verify(m => m.RemoveUserAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RegisterCompanyWorker_CantFindCompanyWorkers_ThrowsExcception()
        {
            _mockProviderService.Setup(m => m.GetCompanyWorkersAsync(It.IsAny<string>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _companyController.GetCompanyWorkers();

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
