using NSubstitute;
using FluentAssertions;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using KartverketGruppe5.Services.Interfaces;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Models.ViewModels;

namespace KartverketGruppe5.Tests.Services
{
    public class SaksbehandlerServiceTests
    {
        private readonly ISaksbehandlerRepository _repository;
        private readonly ILogger<SaksbehandlerService> _logger;
        private readonly IPasswordService _passwordService;
        private readonly SaksbehandlerService _sut;

        public SaksbehandlerServiceTests()
        {
            // Setup for hver test
            _repository = Substitute.For<ISaksbehandlerRepository>();
            _logger = Substitute.For<ILogger<SaksbehandlerService>>();
            _passwordService = Substitute.For<IPasswordService>();
            _sut = new SaksbehandlerService(_repository, _logger, _passwordService);
        }

        [Fact]
        public async Task GetSaksbehandlerById_ExistingId_ReturnsSaksbehandler()
        {
            // Arrange
            var testId = 1;
            var expectedSaksbehandler = new Saksbehandler 
            { 
                SaksbehandlerId = testId,
                Fornavn = "Test",
                Etternavn = "Testesen",
                Email = "test@test.no"
            };
            _repository.GetSaksbehandlerById(testId).Returns(expectedSaksbehandler);

            // Act
            var result = await _sut.GetSaksbehandlerById(testId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSaksbehandler);
            await _repository.Received(1).GetSaksbehandlerById(testId);
        }

        [Fact]
        public async Task GetSaksbehandlerById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var testId = 999;
            _repository.GetSaksbehandlerById(testId).Returns((Saksbehandler?)null);

            // Act
            var result = await _sut.GetSaksbehandlerById(testId);

            // Assert
            result.Should().BeNull();
            await _repository.Received(1).GetSaksbehandlerById(testId);
        }

        [Fact]
        public async Task CreateSaksbehandler_ValidData_ReturnsTrue()
        {
            // Arrange
            var saksbehandler = new Saksbehandler
            {
                Fornavn = "Test",
                Etternavn = "Testesen",
                Email = "test@test.no",
                Passord = "password123"
            };
            var hashedPassword = "hashedPassword";
            _passwordService.HashPassword(saksbehandler.Passord).Returns(hashedPassword);
            _repository.CreateSaksbehandler(Arg.Any<Saksbehandler>()).Returns(true);

            // Act
            var result = await _sut.CreateSaksbehandler(saksbehandler);

            // Assert
            result.Should().BeTrue();
            await _repository.Received(1).CreateSaksbehandler(Arg.Is<Saksbehandler>(
                s => s.Passord == hashedPassword && 
                     s.Email == saksbehandler.Email));
        }

        [Fact]
        public async Task UpdateSaksbehandler_WithNewPassword_UpdatesSuccessfully()
        {
            // Arrange
            var viewModel = new SaksbehandlerRegistrerViewModel
            {
                SaksbehandlerId = 1,
                Fornavn = "Test",
                Etternavn = "Testesen",
                Email = "test@test.no",
                Passord = "newPassword123"
            };
            var hashedPassword = "hashedNewPassword";
            _passwordService.HashPassword(viewModel.Passord).Returns(hashedPassword);
            _repository.UpdateSaksbehandler(Arg.Any<SaksbehandlerRegistrerViewModel>()).Returns(true);

            // Act
            var result = await _sut.UpdateSaksbehandler(viewModel);

            // Assert
            result.Should().BeTrue();
            await _repository.Received(1).UpdateSaksbehandler(Arg.Is<SaksbehandlerRegistrerViewModel>(
                s => s.Passord == hashedPassword));
        }
    }
} 