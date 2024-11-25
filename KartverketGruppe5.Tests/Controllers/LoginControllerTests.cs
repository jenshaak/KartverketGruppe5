using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using FluentAssertions;
using KartverketGruppe5.Controllers;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Tests.Controllers
{
    public class LoginControllerTests
    {
        private readonly IBrukerService _brukerService;
        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly IPasswordService _passwordService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<LoginController> _logger;
        private readonly LoginController _sut;
        private readonly HttpContext _httpContext;
        private readonly ISession _session;

        public LoginControllerTests()
        {
            _brukerService = Substitute.For<IBrukerService>();
            _saksbehandlerService = Substitute.For<ISaksbehandlerService>();
            _passwordService = Substitute.For<IPasswordService>();
            _notificationService = Substitute.For<INotificationService>();
            _logger = Substitute.For<ILogger<LoginController>>();
            
            // Setup HttpContext og Session
            _httpContext = new DefaultHttpContext();
            _session = Substitute.For<ISession>();
            _httpContext.Session = _session;

            // Setup Authentication og andre MVC-tjenester
            var authServiceMock = Substitute.For<IAuthenticationService>();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempDataDictionary = new TempDataDictionary(_httpContext, tempDataProvider);
            var tempDataFactory = Substitute.For<ITempDataDictionaryFactory>();
            tempDataFactory.GetTempData(_httpContext).Returns(tempDataDictionary);
            
            var urlHelperFactory = Substitute.For<IUrlHelperFactory>();
            
            var serviceProviderMock = Substitute.For<IServiceProvider>();
            serviceProviderMock.GetService(typeof(IAuthenticationService)).Returns(authServiceMock);
            serviceProviderMock.GetService(typeof(ITempDataDictionaryFactory)).Returns(tempDataFactory);
            serviceProviderMock.GetService(typeof(ITempDataProvider)).Returns(tempDataProvider);
            serviceProviderMock.GetService(typeof(IUrlHelperFactory)).Returns(urlHelperFactory);
            
            _httpContext.RequestServices = serviceProviderMock;

            _sut = new LoginController(
                _brukerService,
                _saksbehandlerService,
                _passwordService,
                _notificationService,
                _logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async Task HandleLogin_ValidSaksbehandler_RedirectsToHome()
        {
            // Arrange
            var model = new LoginViewModel 
            { 
                Email = "test@test.com", 
                Password = "Password123!" 
            };
            var saksbehandler = new Saksbehandler 
            { 
                Email = model.Email,
                Passord = "hashedPassword",
                SaksbehandlerId = 1,
                Fornavn = "Test",
                Etternavn = "Testesen"
            };

            _sut.ModelState.Clear();
            _saksbehandlerService.GetSaksbehandlerByEmail(model.Email).Returns(saksbehandler);
            _passwordService.VerifyPassword(model.Password, saksbehandler.Passord).Returns(true);

            // Act
            var result = await _sut.HandleLogin(model) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Index");
            result.ControllerName.Should().Be("Home");
        }

        [Fact]
        public async Task HandleLogin_ValidBruker_RedirectsToHome()
        {
            // Arrange
            var model = new LoginViewModel 
            { 
                Email = "test@test.com", 
                Password = "Password123!" 
            };
            var bruker = new Bruker 
            { 
                Email = model.Email,
                Passord = "hashedPassword",
                BrukerId = 1,
                Fornavn = "Test",
                Etternavn = "Testesen"
            };

            _saksbehandlerService.GetSaksbehandlerByEmail(model.Email).Returns((Saksbehandler)null);
            _brukerService.GetBrukerByEmail(model.Email).Returns(bruker);
            _passwordService.VerifyPassword(model.Password, bruker.Passord).Returns(true);

            // Act
            var result = await _sut.HandleLogin(model) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Index");
            result.ControllerName.Should().Be("Home");
        }

        [Fact]
        public async Task HandleLogin_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel 
            { 
                Email = "test@test.com", 
                Password = "WrongPassword" 
            };

            _saksbehandlerService.GetSaksbehandlerByEmail(model.Email).Returns((Saksbehandler)null);
            _brukerService.GetBrukerByEmail(model.Email).Returns((Bruker)null);

            // Act
            var result = await _sut.HandleLogin(model) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.ViewName.Should().Be("Index");
            _sut.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Register_ValidModel_RedirectsToHome()
        {
            // Arrange
            var bruker = new Bruker 
            { 
                Email = "test@test.com",
                Passord = "Password123!",
                Fornavn = "Test",
                Etternavn = "Testesen"
            };

            _sut.ModelState.Clear();
            _brukerService.CreateBruker(Arg.Any<Bruker>()).Returns(true);
            _brukerService.GetBrukerByEmail(bruker.Email).Returns(bruker);

            // Act
            var result = await _sut.Register(bruker) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Index");
            result.ControllerName.Should().Be("Home");
        }

        [Fact]
        public async Task Logout_ClearsSessionAndRedirectsToHome()
        {
            // Act
            var result = await _sut.Logout();
                // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
            _session.Received(1).Clear();
        }
    }
} 