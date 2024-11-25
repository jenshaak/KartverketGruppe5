using NSubstitute;
using FluentAssertions;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.RequestModels;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Tests.Services
{
    public class BrukerServiceTests
    {
        private readonly IBrukerRepository _repository;
        private readonly ILogger<BrukerService> _logger;
        private readonly IPasswordService _passwordService;
        private readonly BrukerService _sut;

        public BrukerServiceTests()
        {
            _repository = Substitute.For<IBrukerRepository>();
            _logger = Substitute.For<ILogger<BrukerService>>();
            _passwordService = Substitute.For<IPasswordService>();
            _sut = new BrukerService(_repository, _logger, _passwordService);
        }

        [Fact]
        public async Task GetBrukerByEmail_ExistingEmail_ReturnsBruker()
        {
            // Arrange
            var email = "test@example.com";
            var expectedBruker = new Bruker 
            { 
                Email = email,
                Fornavn = "Test",
                Etternavn = "Testesen"
            };
            _repository.GetByEmail(email).Returns(expectedBruker);

            // Act
            var result = await _sut.GetBrukerByEmail(email);

            // Assert
            result.Should().BeEquivalentTo(expectedBruker);
            await _repository.Received(1).GetByEmail(email);
        }

        [Fact]
        public async Task CreateBruker_NewEmail_ReturnsTrue()
        {
            // Arrange
            var bruker = new Bruker
            {
                Email = "ny@example.com",
                Passord = "password123",
                Fornavn = "Ny",
                Etternavn = "Bruker"
            };
            var hashedPassword = "hashedPassword123";

            _repository.GetByEmail(bruker.Email).Returns((Bruker)null!);
            _passwordService.HashPassword(bruker.Passord).Returns(hashedPassword);
            _repository.Create(Arg.Any<Bruker>()).Returns(true);

            // Act
            var result = await _sut.CreateBruker(bruker);

            // Assert
            result.Should().BeTrue();
            await _repository.Received(1).Create(Arg.Is<Bruker>(b => 
                b.Email == bruker.Email && 
                b.Passord == hashedPassword));
        }

        [Fact]
        public async Task CreateBruker_ExistingEmail_ReturnsFalse()
        {
            // Arrange
            var bruker = new Bruker
            {
                Email = "eksisterende@example.com",
                Passord = "password123"
            };
            _repository.GetByEmail(bruker.Email).Returns(new Bruker());

            // Act
            var result = await _sut.CreateBruker(bruker);

            // Assert
            result.Should().BeFalse();
            await _repository.DidNotReceive().Create(Arg.Any<Bruker>());
        }

        [Fact]
        public async Task UpdateBruker_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var brukerRequest = new BrukerRequest
            {
                BrukerId = 1,
                Email = "test@example.com",
                Fornavn = "Oppdatert",
                Etternavn = "Bruker"
            };
            var existingBruker = new Bruker
            {
                BrukerId = 1,
                Email = "test@example.com",
                Passord = "hashedPassword"
            };

            _repository.GetByEmail(brukerRequest.Email).Returns(existingBruker);
            _repository.Update(Arg.Any<Bruker>()).Returns(true);

            // Act
            var result = await _sut.UpdateBruker(brukerRequest);

            // Assert
            result.Should().BeTrue();
            await _repository.Received(1).Update(Arg.Is<Bruker>(b => 
                b.BrukerId == brukerRequest.BrukerId && 
                b.Fornavn == brukerRequest.Fornavn));
        }

        [Fact]
        public void VerifyPassword_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var password = "password123";
            var hashedPassword = "hashedPassword123";
            _passwordService.VerifyPassword(password, hashedPassword).Returns(true);

            // Act
            var result = _sut.VerifyPassword(password, hashedPassword);

            // Assert
            result.Should().BeTrue();
            _passwordService.Received(1).VerifyPassword(password, hashedPassword);
        }
    }
} 