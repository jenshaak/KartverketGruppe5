using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using FluentAssertions;
using KartverketGruppe5.Controllers;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly IKommunePopulateService _kommunePopulateService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminController> _logger;
        private readonly AdminController _sut;

        public AdminControllerTests()
        {
            _saksbehandlerService = Substitute.For<ISaksbehandlerService>();
            _kommunePopulateService = Substitute.For<IKommunePopulateService>();
            _notificationService = Substitute.For<INotificationService>();
            _logger = Substitute.For<ILogger<AdminController>>();
            _sut = new AdminController(_saksbehandlerService, _kommunePopulateService, _notificationService, _logger);
        }

        [Fact]
        public async Task Index_ReturnsViewWithPagedResult()
        {
            // Arrange
            var pagedResult = new PagedResult<Saksbehandler>
            {
                Items = new List<Saksbehandler> { new() { SaksbehandlerId = 1, Fornavn = "Test" } },
                TotalItems = 1,
                PageSize = 10,
                CurrentPage = 1
            };
            _saksbehandlerService.GetAllSaksbehandlere(Arg.Any<string>(), Arg.Any<int>()).Returns(pagedResult);

            // Act
            var result = await _sut.Index() as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeOfType<PagedResult<Saksbehandler>>();
            var model = result.Model as PagedResult<Saksbehandler>;
            model!.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task Index_WhenExceptionOccurs_RedirectsToHomeWithError()
        {
            // Arrange
            _saksbehandlerService.GetAllSaksbehandlere(Arg.Any<string>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("Test error"));

            // Act
            var result = await _sut.Index() as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Index");
            result.ControllerName.Should().Be("Home");
            _notificationService.Received(1).AddErrorMessage(Arg.Any<string>());
        }

        [Fact]
        public async Task Register_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var saksbehandler = new Saksbehandler 
            { 
                Fornavn = "Test", 
                Etternavn = "Testesen",
                Email = "test@test.com",
                Passord = "Password123!"
            };
            
            _sut.ModelState.Clear();  // Clear any existing errors
            _saksbehandlerService.CreateSaksbehandler(Arg.Is<Saksbehandler>(s => 
                s.Email == saksbehandler.Email && 
                s.Passord == saksbehandler.Passord))
                .Returns(true);

            // Act
            var result = await _sut.Register(saksbehandler) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Index");
            result.ControllerName.Should().Be("Admin");
            _notificationService.Received(1).AddSuccessMessage("Saksbehandler opprettet!");
        }

        [Fact]
        public async Task Register_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var saksbehandler = new Saksbehandler();
            _sut.ModelState.AddModelError("Fornavn", "Required");

            // Act
            var result = await _sut.Register(saksbehandler) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().Be(saksbehandler);
        }

        [Fact]
        public async Task Rediger_Get_ValidId_ReturnsViewWithModel()
        {
            // Arrange
            var id = 1;
            var saksbehandler = new Saksbehandler { SaksbehandlerId = id, Fornavn = "Test" };
            _saksbehandlerService.GetSaksbehandlerById(id).Returns(saksbehandler);

            // Act
            var result = await _sut.Rediger(id) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeOfType<SaksbehandlerRegistrerViewModel>();
            var viewModel = result.Model as SaksbehandlerRegistrerViewModel;
            viewModel!.SaksbehandlerId.Should().Be(id);
        }
    }
} 