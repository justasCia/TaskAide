using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskAide.API.Controllers;
using TaskAide.API.DTOs;
using TaskAide.API.DTOs.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Services;

namespace TaskAide.UnitTests.ControllersTests
{
    public class UserControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private UserController _userController;

        [SetUp]
        public void SetUp()
        {
            _mockUserService = new Mock<IUserService>();
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            var mapper = mapperConfig.CreateMapper();

            _userController = new UserController(_mockUserService.Object, mapper);

            _userController.ControllerContext = new ControllerContext()
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
        public async Task GetProfile_UserFound_ReturnsOkObjectResult()
        {
            _mockUserService.Setup(m => m.GetUserAsync(It.IsAny<string>())).ReturnsAsync(Builder<User>.CreateNew().Build());

            var result = await _userController.GetProfile() as OkObjectResult;
            var resultValue = result!.Value as UserDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task GetProfile_UserNotFound_ThrowsException()
        {
            _mockUserService.Setup(m => m.GetUserAsync(It.IsAny<string>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _userController.GetProfile();

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
