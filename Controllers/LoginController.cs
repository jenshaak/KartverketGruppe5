using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace KartverketGruppe5.Controllers
{
    public class LoginController : Controller
    {
        private readonly BrukerService _brukerService;

        public LoginController(BrukerService brukerService)
        {
            _brukerService = brukerService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string passord)
        {
            var bruker = await _brukerService.GetBrukerByEmail(email);
            if (bruker != null && _brukerService.VerifyPassword(passord, bruker.Passord))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, bruker.Email),
                    new Claim(ClaimTypes.Role, bruker.Rolle),
                    new Claim("BrukerId", bruker.BrukerId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, 
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                HttpContext.Session.SetString("BrukerNavn", $"{bruker.Fornavn} {bruker.Etternavn}");
                HttpContext.Session.SetString("BrukerEmail", bruker.Email);
                HttpContext.Session.SetInt32("BrukerId", bruker.BrukerId);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Ugyldig email eller passord");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
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