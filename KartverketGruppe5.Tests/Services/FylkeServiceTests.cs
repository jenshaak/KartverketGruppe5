using NSubstitute;
using NSubstitute.ExceptionExtensions;
using FluentAssertions;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Tests.Services
{
    public class FylkeServiceTests
    {
        private readonly IFylkeRepository _repository;
        private readonly ILogger<FylkeService> _logger;
        private readonly FylkeService _sut;

        public FylkeServiceTests()
        {
            _repository = Substitute.For<IFylkeRepository>();
            _logger = Substitute.For<ILogger<FylkeService>>();
            _sut = new FylkeService(_repository, _logger);
        }

        [Fact]
        public async Task GetAllFylker_ReturnsListOfFylker()
        {
            // Arrange
            var expectedFylker = new List<Fylke>
            {
                new() { FylkeId = 1, Navn = "Oslo", FylkeNummer = "03" },
                new() { FylkeId = 2, Navn = "Rogaland", FylkeNummer = "11" }
            };
            _repository.GetAllFylker().Returns(expectedFylker);

            // Act
            var result = await _sut.GetAllFylker();

            // Assert
            result.Should().BeEquivalentTo(expectedFylker);
            await _repository.Received(1).GetAllFylker();
        }

        [Fact]
        public async Task GetAllFylker_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var expectedFylker = new List<Fylke>();
            _repository.GetAllFylker().Returns(expectedFylker);

            // Act
            var result = await _sut.GetAllFylker();

            // Assert
            result.Should().BeEmpty();
            await _repository.Received(1).GetAllFylker();
        }

        [Fact]
        public async Task GetAllFylker_RepositoryThrowsException_LogsAndRethrows()
        {
            // Arrange
            var expectedException = new Exception("Database error");
            _repository.GetAllFylker().ThrowsAsync(expectedException);
                // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllFylker());
            exception.Should().Be(expectedException);
            
            // Verifiser at logger ble kalt med riktige argumenter
            _logger.Received(1).LogError(
                Arg.Any<Exception>(),
                "Uventet feil ved henting av fylker"
            );
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new FylkeService(null!, _logger));
            
            exception.ParamName.Should().Be("repository");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new FylkeService(_repository, null!));
            
            exception.ParamName.Should().Be("logger");
        }
    }
} 