using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
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
            _brukerService = brukerService;
            _saksbehandlerService = saksbehandlerService;
            _logger = logger;
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
            _logger.LogInformation("Innloggingsforsøk for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Ugyldig modell-tilstand ved innlogging");
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
                    
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, saksbehandler.Email),
                        new Claim("SaksbehandlerId", saksbehandler.SaksbehandlerId.ToString()),
                        new Claim("UserType", "Saksbehandler"),
                        new Claim("IsAdmin", saksbehandler.Admin.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    HttpContext.Session.SetString("BrukerNavn", $"{saksbehandler.Fornavn} {saksbehandler.Etternavn}");
                    HttpContext.Session.SetString("BrukerEmail", saksbehandler.Email);
                    HttpContext.Session.SetInt32("SaksbehandlerId", saksbehandler.SaksbehandlerId);

                    _logger.LogInformation("Session og claims satt for saksbehandler: {Email}", model.Email);
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
                    
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, bruker.Email),
                        new Claim("BrukerId", bruker.BrukerId.ToString()),
                        new Claim("UserType", "Bruker")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    HttpContext.Session.SetString("BrukerNavn", $"{bruker.Fornavn} {bruker.Etternavn}");
                    HttpContext.Session.SetString("BrukerEmail", bruker.Email);
                    HttpContext.Session.SetInt32("BrukerId", bruker.BrukerId);

                    _logger.LogInformation("Session og claims satt for bruker: {Email}", model.Email);
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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Bruker bruker)
        {
            if (ModelState.IsValid)
            {
                if (await _brukerService.CreateBruker(bruker))
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Email er allerede i bruk");
            }
            return View(bruker);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
} 