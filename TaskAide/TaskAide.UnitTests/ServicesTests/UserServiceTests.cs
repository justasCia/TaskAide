using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Infrastructure.Services;

namespace TaskAide.UnitTests.ServicesTests
{
    public class UserServiceTests
    {
        private Mock<UserManager<User>> _userManagerMock;

        private UserService _sut;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            _sut = new UserService(_userManagerMock.Object);
        }

        [Test]
        public async Task GetUserAsync_WithExistingUserId_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = Builder<User>.CreateNew().Build();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.GetUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(user);
        }

        [Test]
        public void GetUserAsync_WithNonExistingUserId_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null!);

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserAsync(userId));
        }
    }
}
