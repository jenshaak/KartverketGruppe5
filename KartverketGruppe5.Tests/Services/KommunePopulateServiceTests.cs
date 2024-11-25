using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using FluentAssertions;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.API_Models;
using KartverketGruppe5.Repositories.Interfaces;


namespace KartverketGruppe5.Tests.Services
{
    public class KommunePopulateServiceTests
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KommunePopulateService> _logger;
        private readonly ApiSettings _apiSettings;
        private readonly IKommunePopulateRepository _repository;
        private readonly KommunePopulateService _sut;

        public KommunePopulateServiceTests()
        {
            // Setup HttpClient med base address
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://test.api/")
            };

            _logger = Substitute.For<ILogger<KommunePopulateService>>();
            _apiSettings = new ApiSettings { KommuneInfoApiBaseUrl = "http://test.api" };
            _repository = Substitute.For<IKommunePopulateRepository>();
            
            var apiSettingsOptions = Substitute.For<IOptions<ApiSettings>>();
            apiSettingsOptions.Value.Returns(_apiSettings);
            
            _sut = new KommunePopulateService(_httpClient, _logger, apiSettingsOptions, _repository);
        }

        [Theory]
        [InlineData("0301", "Oslo")]
        [InlineData("1101", "Rogaland")]
        [InlineData("4203", "Agder")]
        public void GetFylkesnavnFromKommunenummer_ValidKommunenummer_ReturnsFylkesnavn(string kommunenummer, string expectedFylke)
        {
            // Act
            var result = _sut.GetFylkesnavnFromKommunenummer(kommunenummer);

            // Assert
            result.Should().Be(expectedFylke);
        }

        [Fact]
         public async Task PopulateFylkerOgKommuner_NoChangesNeeded_ReturnsCorrectMessage()
        {
            // Arrange
            var mockConnection = Substitute.For<IDbConnection>();
            var mockTransaction = Substitute.For<IDbTransaction>();
            
            var existingKommuner = new List<string> { "0301", "1101" };
            var kommuneInfo = new List<KommuneInfo>
            {
                new() { Kommunenummer = "0301", KommunenavnNorsk = "Oslo" },
                new() { Kommunenummer = "1101", KommunenavnNorsk = "Stavanger" }
            };

            // Setup HTTP response
            var handler = new MockHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(kommuneInfo))
            });
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.api/") };

            _repository.CreateConnection().Returns(mockConnection);
            mockConnection.BeginTransaction().Returns(mockTransaction);
            _repository.GetExistingKommuneNumbers().Returns(existingKommuner);
            _repository.GetExistingFylkeNumbers().Returns(new List<string> { "03", "11" });
            _repository.CheckFylkeExists(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDbTransaction>()).Returns(true);
            _repository.CheckKommuneExists(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<IDbTransaction>()).Returns(true);

            var sut = new KommunePopulateService(httpClient, _logger, Options.Create(_apiSettings), _repository);

            // Act
            var result = await sut.PopulateFylkerOgKommuner();

            // Assert
            var expectedMessage = "Alle fylker og kommuner er allerede oppdatert til siste versjon. 2 kommuner og 2 fylker totalt.";
            result.Should().Be(expectedMessage);
            await _repository.Received(1).GetExistingKommuneNumbers();
        }


        [Fact]
        public async Task PopulateFylkerOgKommuner_WithNewKommune_UpdatesDatabase()
        {
            // Arrange
            var mockConnection = Substitute.For<IDbConnection>();
            var mockTransaction = Substitute.For<IDbTransaction>();
            
            var existingKommuner = new List<string> { "0301" };
            var kommuneInfo = new List<KommuneInfo>
            {
                new() { Kommunenummer = "0301", KommunenavnNorsk = "Oslo" },
                new() { Kommunenummer = "1101", KommunenavnNorsk = "Stavanger" } // Ny kommune
            };

            // Setup HTTP response
            var handler = new MockHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(kommuneInfo))
            });
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.api/") };

            _repository.CreateConnection().Returns(mockConnection);
            mockConnection.BeginTransaction().Returns(mockTransaction);
            _repository.GetExistingKommuneNumbers().Returns(existingKommuner);
            _repository.GetExistingFylkeNumbers().Returns(new List<string> { "03" });
            _repository.CheckFylkeExists(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDbTransaction>()).Returns(false);
            _repository.InsertOrUpdateFylke(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDbTransaction>()).Returns(1);

            var sut = new KommunePopulateService(httpClient, _logger, Options.Create(_apiSettings), _repository);

            // Act
            var result = await sut.PopulateFylkerOgKommuner();

            // Assert
            result.Should().Contain("Oppdatert");
            await _repository.Received(1).GetExistingKommuneNumbers();
            await _repository.Received(1).GetExistingFylkeNumbers();
            await _repository.Received().InsertOrUpdateFylke(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IDbTransaction>());
        }

        // Helper class for mocking HttpClient
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public MockHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
} 