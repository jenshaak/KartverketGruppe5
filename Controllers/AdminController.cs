using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly BrukerService _brukerService;
        private readonly KommunePopulateService _kommunePopulateService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            BrukerService brukerService,
            KommunePopulateService kommunePopulateService,
            ILogger<AdminController> logger)
        {
            _brukerService = brukerService;
            _kommunePopulateService = kommunePopulateService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var brukere = await _brukerService.GetAlleBrukere();
            return View(brukere);
        }

        [HttpPost]
        public async Task<IActionResult> PopulateFylkerOgKommuner()
        {
            try
            {
                var result = await _kommunePopulateService.PopulateFylkerOgKommuner();
                TempData["Message"] = result;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PopulateFylkerOgKommuner: {ex.Message}");
                TempData["Error"] = "Failed to populate Fylker og Kommuner";
                return RedirectToAction("Index");
            }
        }
    }
} 