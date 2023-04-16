using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAide.Infrastructure.Services;

namespace TaskAide.UnitTests.ServicesTests
{
    public class EncryptionServiceTests
    {
        private EncryptionService _sut;

        [SetUp]
        public void SetUp()
        {
            byte[] key = Encoding.ASCII.GetBytes("SomeRandomKey123");
            _sut = new EncryptionService(key);
        }

        [Test]
        public void EncryptString_WithValidString_ReturnsEncryptedString()
        {
            // Arrange
            string plainString = "Hello, World!";

            // Act
            string encryptedString = _sut.EncryptString(plainString);

            // Assert
            encryptedString.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void DecryptString_WithValidEncryptedString_ReturnsDecryptedString()
        {
            // Arrange
            string plainString = "Hello, World!";
            string encryptedString = _sut.EncryptString(plainString);

            // Act
            string decryptedString = _sut.DecryptString(encryptedString);

            // Assert
            decryptedString.Should().NotBeNullOrEmpty();
            decryptedString.Should().Be(plainString);
        }

        [Test]
        public void DecryptString_WithInvalidEncryptedString_ThrowsException()
        {
            // Arrange
            byte[] key = Encoding.ASCII.GetBytes("SomeRandomKey123");
            EncryptionService encryptionService = new EncryptionService(key);
            string encryptedString = Convert.ToBase64String(Encoding.ASCII.GetBytes("InvalidString"));

            // Act & Assert
            encryptionService.Invoking(x => x.DecryptString(encryptedString))
                .Should().Throw<ArgumentException>()
                .WithMessage("Specified initialization vector (IV) does not match the block size for this algorithm. (Parameter 'rgbIV')");
        }

        [Test]
        public void EncryptString_WithNullString_ThrowsException()
        {
            // Arrange, Act & Assert
            _sut.Invoking(x => x.EncryptString(null))
                .Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'plainString')");
        }

        [Test]
        public void DecryptString_WithNullString_ThrowsException()
        {
            // Arrange, Act & Assert
            _sut.Invoking(x => x.DecryptString(null))
                .Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'encryptedString')");
        }
    }
}
