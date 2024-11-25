using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using FluentAssertions;
using KartverketGruppe5.Controllers;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Models.Interfaces;
using KartverketGruppe5.Services.Interfaces;


namespace KartverketGruppe5.Tests.Controllers
{
    public class SaksbehandlingControllerTests
    {
        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly IInnmeldingService _innmeldingService;
        private readonly ILokasjonService _lokasjonService;
        private readonly IKommuneService _kommuneService;
        private readonly IFylkeService _fylkeService;
        private readonly ILogger<SaksbehandlingController> _logger;
        private readonly INotificationService _notificationService;
        private readonly SaksbehandlingController _sut;

        public SaksbehandlingControllerTests()
        {
            _saksbehandlerService = Substitute.For<ISaksbehandlerService>();
            _innmeldingService = Substitute.For<IInnmeldingService>();
            _lokasjonService = Substitute.For<ILokasjonService>();
            _kommuneService = Substitute.For<IKommuneService>();
            _fylkeService = Substitute.For<IFylkeService>();
            _logger = Substitute.For<ILogger<SaksbehandlingController>>();
            _notificationService = Substitute.For<INotificationService>();

            _sut = new SaksbehandlingController(
                _saksbehandlerService,
                _innmeldingService,
                _lokasjonService,
                _kommuneService,
                _fylkeService,
                _logger,
                _notificationService);
        }

        [Fact]
        public async Task SearchKommuner_ReturnsJsonResult()
        {
            // Arrange
            var kommuner = new List<Kommune> 
            { 
                new() { KommuneId = 1, Navn = "Test Kommune" } 
            };
            _kommuneService.SearchKommuner("test").Returns(Task.FromResult(kommuner));

            // Act
            var result = await _sut.SearchKommuner("test");

            // Assert
            var jsonResult = result as JsonResult;
            jsonResult.Should().NotBeNull();
            var value = jsonResult!.Value as IEnumerable<object>;
            value.Should().NotBeNull();
            value!.Count().Should().Be(1);
        }

        [Fact]
        public async Task Detaljer_WithValidId_ReturnsViewWithData()
        {
            // Arrange
            var innmeldingId = 1;
            var innmelding = new InnmeldingViewModel { InnmeldingId = innmeldingId, LokasjonId = 1, KommuneId = 1 };
            var lokasjon = new LokasjonViewModel { LokasjonId = 1 };
            var saksbehandlere = new PagedResult<Saksbehandler>
            {
                Items = new List<Saksbehandler> { new() { SaksbehandlerId = 1 } },
                CurrentPage = 1,
                PageSize = 10,
                TotalItems = 1
            };

            _innmeldingService.GetInnmeldingById(innmeldingId).Returns(Task.FromResult(innmelding));
            _lokasjonService.GetLokasjonById(innmelding.LokasjonId).Returns(Task.FromResult(lokasjon));
            _kommuneService.GetKommuneById(innmelding.KommuneId).Returns(Task.FromResult(new Kommune { KommuneId = 1 }));
            _saksbehandlerService.GetAllSaksbehandlere().Returns(Task.FromResult<IPagedResult<Saksbehandler>>(saksbehandlere));

            // Act
            var result = await _sut.Detaljer(innmeldingId);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(innmelding);
            viewResult.ViewData["Lokasjon"].Should().Be(lokasjon);
            viewResult.ViewData["Saksbehandlere"].Should().BeEquivalentTo(saksbehandlere.Items);
        }

        [Fact]
        public async Task Videresend_Success_RedirectsToIndex()
        {
            // Arrange
            var innmeldingId = 1;
            var saksbehandlerId = 1;

            // Act
            var result = await _sut.Videresend(innmeldingId, saksbehandlerId);

            // Assert
            _innmeldingService.Received(1).UpdateInnmeldingStatus(
                innmeldingId, 
                saksbehandlerId, 
                "Under behandling");
            _notificationService.Received(1).AddSuccessMessage(Arg.Any<string>());

            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Behandle_Success_RedirectsToMineSaker()
        {
            // Arrange
            var innmeldingId = 1;
            var saksbehandlerId = 1;
            var claims = new List<Claim>
            {
                new Claim("SaksbehandlerId", saksbehandlerId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var innmelding = new InnmeldingViewModel { InnmeldingId = innmeldingId };
            _innmeldingService.GetInnmeldingById(innmeldingId).Returns(Task.FromResult(innmelding));

            // Act
            var result = await _sut.Behandle(innmeldingId);

            // Assert
            _innmeldingService.Received(1).UpdateInnmeldingStatus(
                innmeldingId, 
                saksbehandlerId, 
                "Under behandling");
            _notificationService.Received(1).AddSuccessMessage(Arg.Any<string>());

            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("MineSaker");
        }
    }
} 