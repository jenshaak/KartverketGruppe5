using System.Security.Cryptography;
using Xunit;
using FluentAssertions;
using KartverketGruppe5.Services;
namespace KartverketGruppe5.Tests.Services
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _sut;

        public PasswordServiceTests()
        {
            _sut = new PasswordService();
        }

        [Fact]
        public void HashPassword_SamePassword_ReturnsSameHash()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hash1 = _sut.HashPassword(password);
            var hash2 = _sut.HashPassword(password);

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void HashPassword_DifferentPasswords_ReturnsDifferentHashes()
        {
            // Arrange
            var password1 = "TestPassword123!";
            var password2 = "TestPassword123!!";

            // Act
            var hash1 = _sut.HashPassword(password1);
            var hash2 = _sut.HashPassword(password2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "TestPassword123!";
            var hashedPassword = _sut.HashPassword(password);

            // Act
            var result = _sut.VerifyPassword(password, hashedPassword);

            // Assert
            result.Should().BeTrue();
        }
    }
} 