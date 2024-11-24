using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using KartverketGruppe5.Services.Interfaces;
namespace KartverketGruppe5.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private const string DefaultErrorMessage = "Ugyldig email eller passord";
        private const string UnexpectedErrorMessage = "En uventet feil oppstod";
        
        private readonly IBrukerService _brukerService;
        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly IPasswordService _passwordService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            IBrukerService brukerService,
            ISaksbehandlerService saksbehandlerService,
            IPasswordService passwordService,
            INotificationService notificationService,
            ILogger<LoginController> logger)
        {
            _brukerService = brukerService ?? throw new ArgumentNullException(nameof(brukerService));
            _saksbehandlerService = saksbehandlerService ?? throw new ArgumentNullException(nameof(saksbehandlerService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _notificationService = notificationService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /Login
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleLogin([FromForm] LoginViewModel model)
        {
            try 
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                // Forsøk innlogging som saksbehandler først
                var loginResult = await TryLoginAsSaksbehandler(model);
                if (loginResult != null) return loginResult;

                // Deretter forsøk innlogging som bruker
                loginResult = await TryLoginAsBruker(model);
                if (loginResult != null) return loginResult;

                ModelState.AddModelError("", DefaultErrorMessage);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil under innlogging for {Email}", model.Email);
                ModelState.AddModelError("", UnexpectedErrorMessage);
                return View("Index", model);
            }
        }

        // Nye private hjelpemetoder for å dele opp loginlogikken
        private async Task<IActionResult?> TryLoginAsSaksbehandler(LoginViewModel model)
        {
            var saksbehandler = await _saksbehandlerService.GetSaksbehandlerByEmail(model.Email);
            if (saksbehandler == null) return null;

            if (!_passwordService.VerifyPassword(model.Password, saksbehandler.Passord))
            {
                _logger.LogWarning("Feil passord for saksbehandler: {Email}", model.Email);
                return null;
            }

            await SignInUser(model.Email, saksbehandler.SaksbehandlerId, 
                "Saksbehandler", saksbehandler.Fornavn, 
                saksbehandler.Etternavn, saksbehandler.Admin);
            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult?> TryLoginAsBruker(LoginViewModel model)
        {
            var bruker = await _brukerService.GetBrukerByEmail(model.Email);
            if (bruker == null) return null;

            if (!_passwordService.VerifyPassword(model.Password, bruker.Passord))
            {
                _logger.LogWarning("Feil passord for bruker: {Email}", model.Email);
                return null;
            }

            await SignInUser(model.Email, bruker.BrukerId, 
                "Bruker", bruker.Fornavn, 
                bruker.Etternavn);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Bruker bruker)
        {
            try
            {
                _logger.LogInformation("Registreringsforsøk for email: {Email}", bruker.Email);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Registrering feilet: Ugyldig modell-tilstand for {Email}", bruker.Email);
                    return View(bruker);
                }

                if (await _brukerService.CreateBruker(bruker))
                {
                    _logger.LogInformation("Bruker registrert med email: {Email}", bruker.Email);
                    
                    // Hent den nylig opprettede brukeren for å få BrukerId
                    var nyBruker = await _brukerService.GetBrukerByEmail(bruker.Email);
                    if (nyBruker == null)
                    {
                        _logger.LogError("Kunne ikke hente nyopprettet bruker: {Email}", bruker.Email);
                        ModelState.AddModelError("", "En feil oppstod under registrering");
                        return View(bruker);
                    }

                    await SignInUser(nyBruker.Email, nyBruker.BrukerId, "Bruker", nyBruker.Fornavn, nyBruker.Etternavn);
                    return RedirectToAction("Index", "Home");
                }

                _logger.LogWarning("Registrering feilet: Email er allerede i bruk: {Email}", bruker.Email);
                ModelState.AddModelError("", "Email er allerede i bruk");
                return View(bruker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil under registrering for {Email}", bruker.Email);
                ModelState.AddModelError("", "En uventet feil oppstod under registrering");
                return View(bruker);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                _logger.LogInformation("Starter utlogging");
                
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                
                _logger.LogInformation("Utlogging fullført");
                _notificationService.AddSuccessMessage("Du er nå utlogget.");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil under utlogging");
                // Selv om det oppstår en feil, prøver vi å redirecte til home
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task SignInUser(string email, int userId, string userType, 
            string fornavn, string etternavn, bool isAdmin = false)
        {
            try 
            {
                var claims = BuildUserClaims(email, userId, userType, isAdmin);
                await SignInWithClaims(claims);
                SetUserSession(userType, fornavn, etternavn, email, userId, isAdmin);
                
                _logger.LogInformation("{UserType} logget inn: {Email} med ID: {UserId}", 
                    userType, email, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil under innlogging av {UserType}: {Email}", 
                    userType, email);
                throw;
            }
        }

        private List<Claim> BuildUserClaims(string email, int userId, 
            string userType, bool isAdmin)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, email),
                new($"{userType}Id", userId.ToString()),
                new("UserType", userType),
                new(ClaimTypes.Role, userType)
            };

            if (isAdmin)
            {
                claims.AddRange(new[]
                {
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("IsAdmin", "True")
                });
            }

            return claims;
        }

        private async Task SignInWithClaims(List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims, 
                CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));
        }

        private void SetUserSession(string userType, string fornavn, 
            string etternavn, string email, int userId, bool isAdmin)
        {
            HttpContext.Session.SetString($"{userType}Navn", $"{fornavn} {etternavn}");
            HttpContext.Session.SetString($"{userType}Email", email);
            HttpContext.Session.SetInt32($"{userType}Id", userId);
            HttpContext.Session.SetString("UserType", userType);

            if (isAdmin)
            {
                HttpContext.Session.SetString("IsAdmin", "True");
            }
        }
    }
} 