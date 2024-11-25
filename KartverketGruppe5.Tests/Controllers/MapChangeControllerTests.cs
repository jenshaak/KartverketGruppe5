using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Threading.Tasks;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services.Interfaces;
using FluentAssertions;
using KartverketGruppe5.Controllers;


namespace KartverketGruppe5.Tests.Controllers
{
    public class MapChangeControllerTests
    {
        private readonly IInnmeldingService _innmeldingService;
        private readonly ILokasjonService _lokasjonService;
        private readonly IBildeService _bildeService;
        private readonly ILogger<MapChangeController> _logger;
        private readonly INotificationService _notificationService;
        private readonly MapChangeController _sut;
        private readonly HttpContext _httpContext;
        private readonly ISession _session;

        public MapChangeControllerTests()
   {
       _innmeldingService = Substitute.For<IInnmeldingService>();
       _lokasjonService = Substitute.For<ILokasjonService>();
       _bildeService = Substitute.For<IBildeService>();
       _notificationService = Substitute.For<INotificationService>();
       _logger = Substitute.For<ILogger<MapChangeController>>();
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
        _sut = new MapChangeController(
           _innmeldingService,
           _lokasjonService,
           _bildeService,
           _notificationService,
           _logger)
       {
           ControllerContext = new ControllerContext
           {
               HttpContext = _httpContext
           }
       };
        // Setup URL Helper
       _sut.Url = urlHelper;
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
        public void Index_WithoutBrukerId_RedirectsToLogin()
        {
            // Arrange
            SetupSessionBrukerId(null);

            // Act
            var result = _sut.Index() as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Index");
            result.ControllerName.Should().Be("Login");
        }

        [Fact]
        public void Index_WithBrukerId_ReturnsView()
        {
            // Arrange
            SetupSessionBrukerId(1);

            // Act
            var result = _sut.Index() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Index_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new LokasjonViewModel();
            _sut.ModelState.AddModelError("", "Test error");

            // Act
            var result = await _sut.Index(model, "test beskrivelse", null) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().Be(model);
        }

        [Fact]
        public async Task Index_Post_ValidModel_RedirectsToMineInnmeldinger()
        {
            // Arrange
            SetupSessionBrukerId(1);
            var model = new LokasjonViewModel 
            { 
                GeoJson = "test",
                Latitude = 59.0,
                Longitude = 10.0,
                GeometriType = "Point"
            };
            var beskrivelse = "test beskrivelse";
            
            _sut.ModelState.Clear();
            _lokasjonService.AddLokasjon(
                Arg.Any<string>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<string>())
                .Returns(Task.FromResult(1));
            _lokasjonService.GetKommuneIdFromCoordinates(Arg.Any<double>(), Arg.Any<double>())
                .Returns(Task.FromResult(1));
            _innmeldingService.CreateInnmelding(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(1));
                // Act
            var result = await _sut.Index(model, beskrivelse, null);
                // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("MineInnmeldinger");
            _notificationService.Received(1).AddSuccessMessage(Arg.Any<string>());
        }

        [Fact]
        public async Task ViewInnmelding_NotFound_ReturnsNotFound()
        {
            // Arrange
            _innmeldingService.GetInnmeldingById(1).Returns((InnmeldingViewModel)null);

            // Act
            var result = await _sut.ViewInnmelding(1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
} 