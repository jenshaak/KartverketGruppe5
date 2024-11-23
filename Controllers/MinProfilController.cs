using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    public class MinProfilController : Controller
    {
        private readonly BrukerService _brukerService;

        public MinProfilController(BrukerService brukerService)
        {
            _brukerService = brukerService;
        }

        
        [HttpGet]

        // --- Sjekker om bruker er i systemet ---
        public async Task<IActionResult> Index()
        {
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Login", "Login");
            }

            // --- Data om bruker hentes gjennom Service filen ---
            var bruker = await _brukerService.GetBrukerByEmail(brukerEmail);

            if (bruker == null)
            {
                return NotFound();
            }

            return View(bruker);

        }

        [HttpPost]
        public async Task<IActionResult> OppdaterBruker(Bruker bruker)
        {
            // Sjekk om brukeren er logget inn
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Login", "Login");
            }

            // Hvis dataene er gyldige, prøv å oppdatere brukerens informasjon og håndter resultatet
            if (ModelState.IsValid)
            {
                var oppdatert = await _brukerService.OppdaterBruker(bruker);

                if (oppdatert)
                {
                    TempData["SuccessMessage"] = "Profilen din ble oppdatert!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Noe gikk galt, prøv igjen.";
                    return View(bruker);
                }
            }

            return View("Index", bruker);
        }
    }
}        
    