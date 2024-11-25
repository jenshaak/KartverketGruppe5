using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Controllers;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;

namespace KartverketGruppe5.Tests.Controllers
{
    public class MineInnmeldingerControllerTests
    {
        private readonly IInnmeldingService _innmeldingService;
        private readonly ILokasjonService _lokasjonService;
        private readonly IKommuneService _kommuneService;
        private readonly IFylkeService _fylkeService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MineInnmeldingerController> _logger;
        private readonly MineInnmeldingerController _sut;
        private readonly HttpContext _httpContext;
        private readonly ISession _session;

        public MineInnmeldingerControllerTests()
        {
            _innmeldingService = Substitute.For<IInnmeldingService>();
            _lokasjonService = Substitute.For<ILokasjonService>();
            _kommuneService = Substitute.For<IKommuneService>();
            _fylkeService = Substitute.For<IFylkeService>();
            _notificationService = Substitute.For<INotificationService>();
            _logger = Substitute.For<ILogger<MineInnmeldingerController>>();

            // Setup HttpContext og Session
            _httpContext = new DefaultHttpContext();
            _session = Substitute.For<ISession>();
            _httpContext.Session = _session;

            // Setup MVC services
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempDataDictionary = new TempDataDictionary(_httpContext, tempDataProvider);
            var tempDataFactory = Substitute.For<ITempDataDictionaryFactory>();
            tempDataFactory.GetTempData(_httpContext).Returns(tempDataDictionary);

            var urlHelper = Substitute.For<IUrlHelper>();
            var urlHelperFactory = Substitute.For<IUrlHelperFactory>();
            urlHelperFactory.GetUrlHelper(Arg.Any<ActionContext>()).Returns(urlHelper);

            var serviceProviderMock = Substitute.For<IServiceProvider>();
            serviceProviderMock.GetService(typeof(ITempDataDictionaryFactory)).Returns(tempDataFactory);
            serviceProviderMock.GetService(typeof(IUrlHelperFactory)).Returns(urlHelperFactory);
            _httpContext.RequestServices = serviceProviderMock;

            _sut = new MineInnmeldingerController(
                _innmeldingService,
                _lokasjonService,
                _kommuneService,
                _fylkeService,
                _notificationService,
                _logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                },
                Url = urlHelper
            };
        }

        private void SetupSessionBrukerId(int? brukerId)
        {
            _session.TryGetValue("BrukerId", out Arg.Any<byte[]>())
                .Returns(x =>
                {
                    if (brukerId.HasValue)
                    {
                        x[1] = BitConverter.GetBytes(brukerId.Value);
                        return true;
                    }
                    return false;
                });
        }

        [Fact]
        public async Task Index_WithoutBrukerId_RedirectsToLogin()
        {
            // Arrange
            SetupSessionBrukerId(null);

            // Act
            var result = await _sut.Index();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Login");
        }

        [Fact]
        public async Task Index_WithBrukerId_ReturnsViewWithData()
        {
            // Arrange
            SetupSessionBrukerId(1);
            var fylker = new List<Fylke> { new Fylke { FylkeId = 1, Navn = "Test Fylke" } };
            _fylkeService.GetAllFylker().Returns(Task.FromResult(fylker));

            // Act
            var result = await _sut.Index();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewData["Fylker"].Should().BeEquivalentTo(fylker);
        }

        [Fact]
        public async Task Detaljer_WithoutBrukerId_RedirectsToLogin()
        {
            // Arrange
            SetupSessionBrukerId(null);

            // Act
            var result = await _sut.Detaljer(1);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Login");
        }

        [Fact]
        public async Task SlettInnmelding_Success_RedirectsToIndex()
        {
            // Arrange
            var innmeldingId = 1;

            // Act
            var result = await _sut.SlettInnmelding(innmeldingId);

            // Assert
            _innmeldingService.Received(1).SlettInnmelding(innmeldingId);
            _notificationService.Received(1).AddSuccessMessage(Arg.Any<string>());
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
        }
    }
} 