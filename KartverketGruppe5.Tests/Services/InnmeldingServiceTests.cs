using NSubstitute;
using FluentAssertions;
using KartverketGruppe5.Services;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace KartverketGruppe5.Tests.Services
{
    public class InnmeldingServiceTests
    {
        private readonly IInnmeldingRepository _repository;
        private readonly ILogger<InnmeldingService> _logger;
        private readonly IBildeService _bildeService;
        private readonly ILokasjonService _lokasjonService;
        private readonly InnmeldingService _sut;

        public InnmeldingServiceTests()
        {
            _repository = Substitute.For<IInnmeldingRepository>();
            _logger = Substitute.For<ILogger<InnmeldingService>>();
            _bildeService = Substitute.For<IBildeService>();
            _lokasjonService = Substitute.For<ILokasjonService>();
            _sut = new InnmeldingService(_repository, _logger, _bildeService, _lokasjonService);
        }

        [Fact]
        public async Task CreateInnmelding_ValidData_ReturnsId()
        {
            // Arrange
            var brukerId = 1;
            var kommuneId = 1;
            var lokasjonId = 1;
            var beskrivelse = "Test beskrivelse";
            var bildeSti = "bilder/test.jpg";
            var expectedId = 1;

            _repository.AddInnmelding(brukerId, kommuneId, lokasjonId, beskrivelse, bildeSti)
                .Returns(expectedId);

            // Act
            var result = await _sut.CreateInnmelding(brukerId, kommuneId, lokasjonId, beskrivelse, bildeSti);

            // Assert
            result.Should().Be(expectedId);
            await _repository.Received(1).AddInnmelding(brukerId, kommuneId, lokasjonId, beskrivelse, bildeSti);
        }

        [Fact]
        public async Task GetInnmeldingById_ExistingId_ReturnsInnmelding()
        {
            // Arrange
            var innmeldingId = 1;
            var expectedInnmelding = new InnmeldingViewModel
            {
                InnmeldingId = innmeldingId,
                Beskrivelse = "Test beskrivelse",
                Status = "Ny"
            };

            _repository.GetInnmeldingById(innmeldingId).Returns(expectedInnmelding);

            // Act
            var result = await _sut.GetInnmeldingById(innmeldingId);

            // Assert
            result.Should().BeEquivalentTo(expectedInnmelding);
            await _repository.Received(1).GetInnmeldingById(innmeldingId);
        }

        [Fact]
        public async Task GetInnmeldingById_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var innmeldingId = 999;
            _repository.GetInnmeldingById(innmeldingId).Returns((InnmeldingViewModel)null!);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetInnmeldingById(innmeldingId));
        }

        [Fact]
        public async Task UpdateInnmeldingStatus_ValidData_ReturnsTrue()
        {
            // Arrange
            var innmeldingId = 1;
            var saksbehandlerId = 1;
            var status = "Under behandling";

            _repository.UpdateInnmelding(Arg.Any<InnmeldingUpdateModel>()).Returns(true);

            // Act
            var result = await _sut.UpdateInnmeldingStatus(innmeldingId, saksbehandlerId, status);

            // Assert
            result.Should().BeTrue();
            await _repository.Received(1).UpdateInnmelding(Arg.Is<InnmeldingUpdateModel>(
                model => model.InnmeldingId == innmeldingId && 
                        model.SaksbehandlerId == saksbehandlerId && 
                        model.Status == status));
        }

        [Fact]
        public async Task UpdateBilde_ValidData_UpdatesSuccessfully()
        {
            // Arrange
            var innmeldingId = 1;
            var bilde = Substitute.For<IFormFile>();
            var nyBildeSti = "bilder/nytt.jpg";
            var innmelding = new InnmeldingViewModel { InnmeldingId = innmeldingId };

            _repository.GetInnmeldingById(innmeldingId).Returns(innmelding);
            _bildeService.LagreBilde(bilde, innmeldingId).Returns(nyBildeSti);

            // Act
            await _sut.UpdateBilde(innmeldingId, bilde);

            // Assert
            await _bildeService.Received(1).LagreBilde(bilde, innmeldingId);
            await _repository.Received(1).UpdateBildeSti(innmeldingId, nyBildeSti);
        }
    }
} 