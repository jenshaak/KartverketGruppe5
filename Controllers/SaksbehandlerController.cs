using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
    public class SaksbehandlerController : Controller
    {
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly ILogger<SaksbehandlerController> _logger;

        public SaksbehandlerController(SaksbehandlerService saksbehandlerService, ILogger<SaksbehandlerController> logger)
        {
            _saksbehandlerService = saksbehandlerService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Saksbehandler saksbehandler)
        {
            if (ModelState.IsValid)
            {
                var success = await _saksbehandlerService.CreateSaksbehandler(saksbehandler);
                if (!success)
                {
                    _logger.LogError("Registrering feilet for bruker med epost: {Email}", saksbehandler.Email);
                    ModelState.AddModelError("", "Registrering feilet. Prøv igjen.");
                }
                return RedirectToAction("Index", "Home");
            }
            return View(saksbehandler);
        }

        [HttpPost]
        public async Task<IActionResult> EndreRolle(int saksbehandlerId, string nyRolle)
        {
            var success = await _saksbehandlerService.UpdateSaksbehandlerRolle(saksbehandlerId, nyRolle);
            if (!success)
            {
                _logger.LogError("Kunne ikke oppdatere rolle til {NyRolle} for bruker med ID: {SaksbehandlerId}", nyRolle, saksbehandlerId);
                return BadRequest("Kunne ikke oppdatere brukerrolle");
            }
            return RedirectToAction("Index", "Admin");
        }
    }
}
