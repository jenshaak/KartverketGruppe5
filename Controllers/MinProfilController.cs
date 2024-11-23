using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services.Interfaces;

namespace KartverketGruppe5.Controllers
{
    public class MinProfilController : Controller
    {
        private readonly IBrukerService _brukerService;

        public MinProfilController(IBrukerService brukerService)
        {
            _brukerService = brukerService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Login", "Login");
            }

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
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Login", "Login");
            }

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

            return View(bruker);
        }
    }
}        
    