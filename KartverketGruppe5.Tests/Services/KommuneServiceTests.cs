using NSubstitute;
using FluentAssertions;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Tests.Services
{
    public class KommuneServiceTests
    {
        private readonly IKommuneRepository _repository;
        private readonly ILogger<KommuneService> _logger;
        private readonly KommuneService _sut;

        public KommuneServiceTests()
        {
            _repository = Substitute.For<IKommuneRepository>();
            _logger = Substitute.For<ILogger<KommuneService>>();
            _sut = new KommuneService(_repository, _logger);
        }

        [Fact]
        public async Task GetKommuneById_ExistingId_ReturnsKommune()
        {
            // Arrange
            var kommuneId = 4203;
            var expectedKommune = new Kommune 
            { 
                KommuneId = kommuneId,
                Navn = "Arendal",
                KommuneNummer = "4203",
                FylkeId = 42
            };
            _repository.GetKommuneById(kommuneId).Returns(expectedKommune);

            // Act
            var result = await _sut.GetKommuneById(kommuneId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedKommune);
            await _repository.Received(1).GetKommuneById(kommuneId);
        }

        [Fact]
        public async Task GetKommuneById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var kommuneId = 9999;
            _repository.GetKommuneById(kommuneId).Returns((Kommune?)null);

            // Act
            var result = await _sut.GetKommuneById(kommuneId);

            // Assert
            result.Should().BeNull();
            await _repository.Received(1).GetKommuneById(kommuneId);
        }

        [Fact]
        public async Task GetAllKommuner_ReturnsListOfKommuner()
        {
            // Arrange
            var expectedKommuner = new List<Kommune>
            {
                new() { KommuneId = 4203, Navn = "Arendal", KommuneNummer = "4203", FylkeId = 42 },
                new() { KommuneId = 4204, Navn = "Kristiansand", KommuneNummer = "4204", FylkeId = 42 }
            };
            _repository.GetAllKommuner().Returns(expectedKommuner);

            // Act
            var result = await _sut.GetAllKommuner();

            // Assert
            result.Should().BeEquivalentTo(expectedKommuner);
            await _repository.Received(1).GetAllKommuner();
        }

        [Fact]
        public async Task SearchKommuner_WithValidSearchTerm_ReturnsMatchingKommuner()
        {
            // Arrange
            var searchTerm = "Aren";
            var expectedKommuner = new List<Kommune>
            {
                new() { KommuneId = 4203, Navn = "Arendal", KommuneNummer = "4203", FylkeId = 42 }
            };
            _repository.SearchKommuner(searchTerm).Returns(expectedKommuner);

            // Act
            var result = await _sut.SearchKommuner(searchTerm);

            // Assert
            result.Should().BeEquivalentTo(expectedKommuner);
            await _repository.Received(1).SearchKommuner(searchTerm);
        }

        [Fact]
        public async Task SearchKommuner_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var searchTerm = "XYZ";
            _repository.SearchKommuner(searchTerm).Returns(new List<Kommune>());

            // Act
            var result = await _sut.SearchKommuner(searchTerm);

            // Assert
            result.Should().BeEmpty();
            await _repository.Received(1).SearchKommuner(searchTerm);
        }
    }
} 