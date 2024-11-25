using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Controllers;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.RequestModels;
using Microsoft.AspNetCore.Http;

namespace KartverketGruppe5.Tests.Controllers
{
    public class MinProfilControllerTests
    {
        private readonly IBrukerService _brukerService;
        private readonly ILogger<MinProfilController> _logger;
        private readonly INotificationService _notificationService;
        private readonly MinProfilController _sut;
        private readonly HttpContext _httpContext;
        private readonly ISession _session;

        public MinProfilControllerTests()
        {
            _brukerService = Substitute.For<IBrukerService>();
            _logger = Substitute.For<ILogger<MinProfilController>>();
            _notificationService = Substitute.For<INotificationService>();

            _httpContext = new DefaultHttpContext();
            _session = Substitute.For<ISession>();
            _httpContext.Session = _session;

            _sut = new MinProfilController(
                _brukerService,
                _logger,
                _notificationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
        }

        private void SetupSessionEmail(string? email)
        {
            if (email != null)
            {
                byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes(email);
                _session.TryGetValue("BrukerEmail", out Arg.Any<byte[]>())
                    .Returns(x => {
                        x[1] = emailBytes;
                        return true;
                    });
            }
            else
            {
                _session.TryGetValue("BrukerEmail", out Arg.Any<byte[]>())
                    .Returns(x => {
                        x[1] = null!;
                        return false;
                    });
            }
        }

        [Fact]
        public async Task Index_WithoutEmail_RedirectsToLogin()
        {
            // Arrange
            SetupSessionEmail(null);

            // Act
            var result = await _sut.Index();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("Login");
        }

        [Fact]
        public async Task Index_WithInvalidEmail_RedirectsToLogin()
        {
            // Arrange
            var email = "test@test.com";
            SetupSessionEmail(email);
            _brukerService.GetBrukerByEmail(email).Returns(Task.FromResult<Bruker>(null!));

            // Act
            var result = await _sut.Index();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("Login");
        }

        [Fact]
        public async Task Index_WithValidEmail_ReturnsView()
        {
            // Arrange
            var email = "test@test.com";
            var bruker = new Bruker { Email = email };
            SetupSessionEmail(email);
            _brukerService.GetBrukerByEmail(email).Returns(Task.FromResult(bruker));

            // Act
            var result = await _sut.Index();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(bruker);
        }

        [Fact]
        public async Task OppdaterBruker_WithoutEmail_RedirectsToLogin()
        {
            // Arrange
            SetupSessionEmail(null);
            var brukerRequest = new BrukerRequest();

            // Act
            var result = await _sut.OppdaterBruker(brukerRequest);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Login");
        }

        [Fact]
        public async Task OppdaterBruker_WithInvalidModel_ReturnsView()
        {
            // Arrange
            SetupSessionEmail("test@test.com");
            var brukerRequest = new BrukerRequest();
            _sut.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = await _sut.OppdaterBruker(brukerRequest);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(brukerRequest);
            _notificationService.Received(1).AddErrorMessage(Arg.Any<string>());
        }

        [Fact]
        public async Task OppdaterBruker_Success_RedirectsToIndex()
        {
            // Arrange
            SetupSessionEmail("test@test.com");
            var brukerRequest = new BrukerRequest();
            _brukerService.UpdateBruker(brukerRequest).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.OppdaterBruker(brukerRequest);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            _notificationService.Received(1).AddSuccessMessage(Arg.Any<string>());
        }

        [Fact]
        public async Task SlettBruker_WithoutEmail_RedirectsToLogin()
        {
            // Arrange
            SetupSessionEmail(null);

            // Act
            var result = await _sut.SlettBruker(1);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Login");
        }

        [Fact]
        public async Task SlettBruker_Success_RedirectsToLogin()
        {
            // Arrange
            SetupSessionEmail("test@test.com");
            _brukerService.DeleteBruker(1).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.SlettBruker(1);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Login");
            _notificationService.Received(1).AddSuccessMessage(Arg.Any<string>());
        }
    }
} 