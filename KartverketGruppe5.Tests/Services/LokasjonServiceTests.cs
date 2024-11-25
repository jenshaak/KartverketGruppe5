using NSubstitute;
using FluentAssertions;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Tests.Services
{
    public class LokasjonServiceTests
    {
        private readonly ILokasjonRepository _repository;
        private readonly ILogger<LokasjonService> _logger;
        private readonly LokasjonService _sut;

        public LokasjonServiceTests()
        {
            _repository = Substitute.For<ILokasjonRepository>();
            _logger = Substitute.For<ILogger<LokasjonService>>();
            _sut = new LokasjonService(_repository, _logger);
        }

        [Fact]
        public async Task GetAllLokasjoner_ReturnsListOfLokasjoner()
        {
            // Arrange
            var expectedLokasjoner = new List<LokasjonViewModel>
            {
                new() { LokasjonId = 1, Latitude = 58.34, Longitude = 8.59, GeometriType = "point" },
                new() { LokasjonId = 2, Latitude = 59.91, Longitude = 10.75, GeometriType = "marker" }
            };
            _repository.GetAllLokasjoner().Returns(expectedLokasjoner);

            // Act
            var result = await _sut.GetAllLokasjoner();

            // Assert
            result.Should().BeEquivalentTo(expectedLokasjoner);
            await _repository.Received(1).GetAllLokasjoner();
        }

        [Fact]
        public async Task AddLokasjon_ValidData_ReturnsId()
        {
            // Arrange
            var geoJson = "{\"type\":\"Point\",\"coordinates\":[8.59,58.34]}";
            var latitude = 58.34;
            var longitude = 8.59;
            var geometriType = "point";
            var expectedId = 1;

            _repository.AddLokasjon(geoJson, latitude, longitude, geometriType).Returns(expectedId);

            // Act
            var result = await _sut.AddLokasjon(geoJson, latitude, longitude, geometriType);

            // Assert
            result.Should().Be(expectedId);
            await _repository.Received(1).AddLokasjon(geoJson, latitude, longitude, geometriType);
        }

        [Theory]
        [InlineData(-91, 0)]    // Invalid latitude (too low)
        [InlineData(91, 0)]     // Invalid latitude (too high)
        [InlineData(0, -181)]   // Invalid longitude (too low)
        [InlineData(0, 181)]    // Invalid longitude (too high)
        public async Task AddLokasjon_InvalidCoordinates_ThrowsArgumentOutOfRangeException(double latitude, double longitude)
        {
            // Arrange
            var geoJson = "{\"type\":\"Point\",\"coordinates\":[0,0]}";
            var geometriType = "point";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
                _sut.AddLokasjon(geoJson, latitude, longitude, geometriType));
        }

        [Fact]
        public async Task AddLokasjon_InvalidGeometriType_ThrowsArgumentException()
        {
            // Arrange
            var geoJson = "{\"type\":\"Point\",\"coordinates\":[8.59,58.34]}";
            var latitude = 58.34;
            var longitude = 8.59;
            var invalidGeometriType = "invalid_type";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.AddLokasjon(geoJson, latitude, longitude, invalidGeometriType));
        }

        [Fact]
        public async Task GetKommuneIdFromCoordinates_ValidCoordinates_ReturnsKommuneId()
        {
            // Arrange
            var latitude = 58.34;
            var longitude = 8.59;
            var expectedKommuneId = 4203;

            _repository.GetKommuneIdFromCoordinates(latitude, longitude).Returns(expectedKommuneId);

            // Act
            var result = await _sut.GetKommuneIdFromCoordinates(latitude, longitude);

            // Assert
            result.Should().Be(expectedKommuneId);
            await _repository.Received(1).GetKommuneIdFromCoordinates(latitude, longitude);
        }

        [Fact]
        public async Task UpdateLokasjon_ValidData_UpdatesSuccessfully()
        {
            // Arrange
            var lokasjon = new LokasjonViewModel 
            { 
                LokasjonId = 1,
                Latitude = 58.34,
                Longitude = 8.59,
                GeometriType = "point"
            };
            var oppdatertDato = DateTime.Now;

            // Act
            await _sut.UpdateLokasjon(lokasjon, oppdatertDato);

            // Assert
            await _repository.Received(1).UpdateLokasjon(
                Arg.Is<LokasjonViewModel>(l => l.LokasjonId == lokasjon.LokasjonId),
                Arg.Is<DateTime>(d => d == oppdatertDato));
        }

        [Fact]
        public async Task GetLokasjonById_ExistingId_ReturnsLokasjon()
        {
            // Arrange
            var lokasjonId = 1;
            var expectedLokasjon = new LokasjonViewModel 
            { 
                LokasjonId = lokasjonId,
                Latitude = 58.34,
                Longitude = 8.59,
                GeometriType = "point"
            };

            _repository.GetLokasjonById(lokasjonId).Returns(expectedLokasjon);

            // Act
            var result = await _sut.GetLokasjonById(lokasjonId);

            // Assert
            result.Should().BeEquivalentTo(expectedLokasjon);
            await _repository.Received(1).GetLokasjonById(lokasjonId);
        }
    }
} 