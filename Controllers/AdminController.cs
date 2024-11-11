using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    public class AdminController : Controller
    {
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly KommunePopulateService _kommunePopulateService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            SaksbehandlerService saksbehandlerService,
            KommunePopulateService kommunePopulateService,
            ILogger<AdminController> logger)
        {
            _saksbehandlerService = saksbehandlerService;
            _kommunePopulateService = kommunePopulateService;
            _logger = logger;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Saksbehandler saksbehandler)
        {
            if (!ModelState.IsValid)
            {
                return View(saksbehandler);
            }

            try
            {
                // TODO: Hash passord før lagring
                var result = await _saksbehandlerService.CreateSaksbehandler(saksbehandler);
                if (result)
                {
                    TempData["Success"] = "Saksbehandler opprettet!";
                    return RedirectToAction("Index", "Admin");
                }
                
                ModelState.AddModelError("", "Kunne ikke opprette saksbehandler");
                return View(saksbehandler);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating saksbehandler: {ex.Message}");
                ModelState.AddModelError("", "En feil oppstod ved registrering");
                return View(saksbehandler);
            }
        }

        public async Task<IActionResult> Index()
        {
            var saksbehandlere = await _saksbehandlerService.GetAllSaksbehandlere();
            return View(saksbehandlere);
        }

        [HttpPost]
        public async Task<IActionResult> PopulateFylkerOgKommuner()
        {
            try
            {
                var result = await _kommunePopulateService.PopulateFylkerOgKommuner();
                TempData["Message"] = result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Feil under oppdatering av fylker og kommuner: {ex.Message}");
                TempData["Error"] = "Det oppstod en feil under oppdatering av fylker og kommuner.";
            }

            return RedirectToAction("Index");
        }
    }
} 