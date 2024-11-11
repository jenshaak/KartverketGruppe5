using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
    public class SaksbehandlingController : Controller
    {
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly ILogger<SaksbehandlingController> _logger;

        public SaksbehandlingController(SaksbehandlerService saksbehandlerService, ILogger<SaksbehandlingController> logger)
        {
            _saksbehandlerService = saksbehandlerService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Henter alle innmeldinger");
            var innmeldinger = _saksbehandlerService.GetAllInnmeldinger();
            _logger.LogInformation("Fant {AntallInnmeldinger} innmeldinger", innmeldinger.Count);
            return View(innmeldinger);
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

        public async Task<IActionResult> Detaljer(int id)
        {
            try 
            {
                var innmelding = await _saksbehandlerService.GetInnmeldingById(id);
                if (innmelding == null)
                {
                    _logger.LogWarning("Fant ikke innmelding med ID: {Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("Hentet detaljer for innmelding {Id}", id);
                return View(innmelding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmelding {Id}", id);
                return RedirectToAction("Index");
            }
        }
    }
}
