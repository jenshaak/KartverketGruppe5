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
                return RedirectToAction("Login", "Auth");
            }

            // --- Data om bruker hentes gjennom Service filen ---
            var bruker = await _brukerService.GetBrukerByEmail(brukerEmail);

            if (bruker == null)
            {
                return NotFound();
            }

            return View(bruker);

        }
    }
}
