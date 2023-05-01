using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAide.API.Controllers;
using TaskAide.API.DTOs.Auth;
using TaskAide.API.Services.Auth;
using TaskAide.Domain.Exceptions;

namespace TaskAide.UnitTests.ControllersTests
{
    public class AuthControllerTests
    {
        private AuthController _authController;
        private Mock<IAuthService> _mockAuthService;

        [SetUp]
        public void Setup()
        {
            _mockAuthService = new Mock<IAuthService>();
            _authController = new AuthController(_mockAuthService.Object);
        }

        [Test]
        public async Task RegisterUserAsync_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var registerUser = Builder<RegisterUserDto>.CreateNew().Build();
            _mockAuthService.Setup(m => m.RegisterUserAsync(registerUser, RegisterType.User))
                .ReturnsAsync(new Domain.Entities.Users.User { Email = registerUser.Email });

            // Act
            var result = await _authController.Register(registerUser) as OkObjectResult;
            var userDto = result!.Value as UserDto;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            userDto.Should().NotBeNull();
            userDto!.Email.Should().Be(registerUser.Email);
        }

        [Test]
        public async Task RegisterUserAsync_InvalidInput_ThrowsException()
        {
            // Arrange
            var regiserUser = Builder<RegisterUserDto>.CreateNew().Build();
            _mockAuthService.Setup(m => m.RegisterUserAsync(regiserUser, RegisterType.User)).ThrowsAsync(new BadRequestException(""));

            // Act
            var act = async () => await _authController.Register(regiserUser); 

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        public async Task RegisterProvider_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var registerUser = Builder<RegisterUserDto>.CreateNew().Build();
            _mockAuthService.Setup(m => m.RegisterUserAsync(registerUser, RegisterType.Provider))
                .ReturnsAsync(new Domain.Entities.Users.User { Email = registerUser.Email });

            // Act
            var result = await _authController.RegisterProvider(registerUser) as OkObjectResult;
            var userDto = result!.Value as UserDto;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            userDto.Should().NotBeNull();
            userDto!.Email.Should().Be(registerUser.Email);
        }

        [Test]
        public async Task RegisterProvider_InvalidInput_ThrowsException()
        {
            // Arrange
            var registerUser = Builder<RegisterUserDto>.CreateNew().Build();
            _mockAuthService.Setup(m => m.RegisterUserAsync(registerUser, RegisterType.Provider)).ThrowsAsync(new BadRequestException(""));

            // Act
            var act = async () => await _authController.RegisterProvider(registerUser);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }


        [Test]
        public async Task RegisterCompany_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var registerCompany = Builder<RegisterCompanyDto>.CreateNew().Build();
            _mockAuthService.Setup(m => m.RegisterCompanyAsync(registerCompany))
                .ReturnsAsync(new UserDto { Email = registerCompany.Email });

            // Act
            var result = await _authController.RegisterCompany(registerCompany) as OkObjectResult;
            var companyDto = result!.Value as UserDto;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            companyDto.Should().NotBeNull();
            companyDto!.Email.Should().Be(registerCompany.Email);
        }

        [Test]
        public async Task RegisterCompany_InvalidInput_ThrowsException()
        {
            // Arrange
            var registerCompany = Builder<RegisterCompanyDto>.CreateNew().Build();
            _mockAuthService.Setup(m => m.RegisterCompanyAsync(registerCompany)).ThrowsAsync(new BadRequestException(""));

            // Act
            var act = async () => await _authController.RegisterCompany(registerCompany);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}
