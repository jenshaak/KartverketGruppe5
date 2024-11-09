using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    public class AdminController : Controller
    {
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            SaksbehandlerService saksbehandlerService, 
            ILogger<AdminController> logger)
        {
            _saksbehandlerService = saksbehandlerService;
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
    }
} 