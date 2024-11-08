using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
    public class BrukerController : Controller
    {
        private readonly BrukerService _brukerService;
        private readonly ILogger<BrukerController> _logger;

        public BrukerController(BrukerService brukerService, ILogger<BrukerController> logger)
        {
            _brukerService = brukerService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Bruker bruker)
        {
            if (ModelState.IsValid)
            {
                var success = await _brukerService.CreateBruker(bruker);
                if (!success)
                {
                    _logger.LogError("Registrering feilet for bruker med epost: {Email}", bruker.Email);
                    ModelState.AddModelError("", "Registrering feilet. Prøv igjen.");
                }
                return RedirectToAction("Index", "Home");
            }
            return View(bruker);
        }

        [HttpPost]
        public async Task<IActionResult> EndreRolle(int brukerId, string nyRolle)
        {
            var success = await _brukerService.OppdaterBrukerRolle(brukerId, nyRolle);
            if (!success)
            {
                _logger.LogError("Kunne ikke oppdatere rolle til {NyRolle} for bruker med ID: {BrukerId}", nyRolle, brukerId);
                return BadRequest("Kunne ikke oppdatere brukerrolle");
            }
            return RedirectToAction("Index", "Admin");
        }
    }
}
