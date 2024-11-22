using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace KartverketGruppe5.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly BrukerService _brukerService;
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            BrukerService brukerService,
            SaksbehandlerService saksbehandlerService,
            ILogger<LoginController> logger)
        {
            _brukerService = brukerService ?? throw new ArgumentNullException(nameof(brukerService));
            _saksbehandlerService = saksbehandlerService ?? throw new ArgumentNullException(nameof(saksbehandlerService));
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
                _logger.LogInformation("Innloggingsforsøk startet for email: {Email}", model.Email);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Innlogging feilet: Ugyldig modell-tilstand for {Email}", model.Email);
                    return View("Index", model);
                }

                // Sjekk først om det er en saksbehandler
                var saksbehandler = await _saksbehandlerService.GetSaksbehandlerByEmail(model.Email);
                if (saksbehandler != null)
                {
                    _logger.LogInformation("Fant saksbehandler med email: {Email}", model.Email);
                    
                    if (_saksbehandlerService.VerifyPassword(model.Password, saksbehandler.Passord))
                    {
                        _logger.LogInformation("Vellykket innlogging for saksbehandler: {Email}", model.Email);
                        
                        await SignInUser(model.Email, saksbehandler.SaksbehandlerId, "Saksbehandler", saksbehandler.Fornavn, saksbehandler.Etternavn, saksbehandler.Admin);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogWarning("Feil passord for saksbehandler: {Email}", model.Email);
                    }
                }

                // Hvis ikke saksbehandler, sjekk om det er en vanlig bruker
                var bruker = await _brukerService.GetBrukerByEmail(model.Email);
                if (bruker != null)
                {
                    _logger.LogInformation("Fant bruker med email: {Email}", model.Email);
                    
                    if (_brukerService.VerifyPassword(model.Password, bruker.Passord))
                    {
                        _logger.LogInformation("Vellykket innlogging for bruker: {Email}", model.Email);
                        
                        await SignInUser(model.Email, bruker.BrukerId, "Bruker", bruker.Fornavn, bruker.Etternavn);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogWarning("Feil passord for bruker: {Email}", model.Email);
                    }
                }
                else
                {
                    _logger.LogWarning("Ingen bruker eller saksbehandler funnet med email: {Email}", model.Email);
                }

                ModelState.AddModelError("", "Ugyldig email eller passord");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil under innlogging for {Email}", model.Email);
                ModelState.AddModelError("", "En uventet feil oppstod under innlogging");
                return View("Index", model);
            }
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
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil under utlogging");
                // Selv om det oppstår en feil, prøver vi å redirecte til home
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task SignInUser(string email, int userId, string userType, string fornavn, string etternavn, bool isAdmin = false)
        {
            try 
            {
                _logger.LogInformation("Starter innlogging for {UserType}: {Email}", userType, email);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim($"{userType}Id", userId.ToString()),
                    new Claim("UserType", userType),
                    new Claim(ClaimTypes.Role, userType)
                };

                if (isAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    claims.Add(new Claim("IsAdmin", "True"));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Sett session-variabler
                HttpContext.Session.SetString($"{userType}Navn", $"{fornavn} {etternavn}");
                HttpContext.Session.SetString($"{userType}Email", email);
                HttpContext.Session.SetInt32($"{userType}Id", userId);
                HttpContext.Session.SetString("UserType", userType);

                if (isAdmin)
                {
                    HttpContext.Session.SetString("IsAdmin", "True");
                }

                _logger.LogInformation("{UserType} logget inn: {Email} med ID: {UserId}", userType, email, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil under innlogging av {UserType}: {Email}", userType, email);
                throw;
            }
        }
    }
} 