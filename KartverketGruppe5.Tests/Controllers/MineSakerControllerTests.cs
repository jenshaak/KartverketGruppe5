using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using FluentAssertions;
using KartverketGruppe5.Controllers;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.Interfaces;

namespace KartverketGruppe5.Tests.Controllers
{
    public class MineSakerControllerTests
    {
        private readonly IInnmeldingService _innmeldingService;
        private readonly ILokasjonService _lokasjonService;
        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly IFylkeService _fylkeService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MineSakerController> _logger;
        private readonly MineSakerController _sut;
        private readonly HttpContext _httpContext;
        private readonly ISession _session;

        public MineSakerControllerTests()
        {
            _innmeldingService = Substitute.For<IInnmeldingService>();
            _lokasjonService = Substitute.For<ILokasjonService>();
            _saksbehandlerService = Substitute.For<ISaksbehandlerService>();
            _fylkeService = Substitute.For<IFylkeService>();
            _notificationService = Substitute.For<INotificationService>();
            _logger = Substitute.For<ILogger<MineSakerController>>();

            // Setup HttpContext og Claims
            _httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("SaksbehandlerId", "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContext.User = claimsPrincipal;

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

            _sut = new MineSakerController(
                _innmeldingService,
                _lokasjonService,
                _fylkeService,
                _saksbehandlerService,
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

        [Fact]
        public async Task Index_WithoutSaksbehandlerId_RedirectsToLogin()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _sut.Index();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Login");
        }

        [Fact]
        public async Task Index_WithSaksbehandlerId_ReturnsViewWithData()
        {
            // Arrange
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
        public async Task Behandle_WithalidId_ReturnsViewWithData()
        {
            // Arrange
   var innmeldingId = 1;
   var innmelding = new InnmeldingViewModel { InnmeldingId = innmeldingId, LokasjonId = 1 };
   var lokasjon = new LokasjonViewModel { LokasjonId = 1 };
   var saksbehandlere = new List<Saksbehandler> 
   { 
       new Saksbehandler { SaksbehandlerId = 1 } 
   };
    _innmeldingService
       .GetInnmeldingById(innmeldingId)
       .Returns(Task.FromResult(innmelding));
    _lokasjonService
       .GetLokasjonById(innmelding.LokasjonId)
       .Returns(Task.FromResult(lokasjon));
    var pagedResult = new PagedResult<Saksbehandler>
   {
       Items = saksbehandlere,
       CurrentPage = 1,
       PageSize = 10,
       TotalItems = saksbehandlere.Count
   };
    _saksbehandlerService
       .GetAllSaksbehandlere()
       .Returns(Task.FromResult<IPagedResult<Saksbehandler>>(pagedResult));
    // Act
   var result = await _sut.Behandle(innmeldingId);
    // Assert
   var viewResult = result as ViewResult;
   viewResult.Should().NotBeNull();
   viewResult!.Model.Should().Be(innmelding);
   viewResult.ViewData["Lokasjon"].Should().Be(lokasjon);
   viewResult.ViewData["Saksbehandlere"].As<IPagedResult<Saksbehandler>>().Items
       .Should().BeEquivalentTo(saksbehandlere);
        }

        [Fact]
        public async Task FullforBehandling_Success_RedirectsToIndex()
        {
            // Arrange
   var innmeldingId = 1;
   var kommentar = "Test kommentar";
   var status = "Fullført";
    // Act
   var result = await _sut.FullforBehandling(innmeldingId, kommentar, status);
    // Assert
            _innmeldingService.Received(1).UpdateStatusAndKommentar(innmeldingId, kommentar, status);
            _notificationService.Received(1).AddSuccessMessage(Arg.Any<string>());
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
        }
    }
} 