using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
    public class MinProfilController : Controller
    {
        private readonly IBrukerService _brukerService;
        private readonly ILogger<MinProfilController> _logger;
        private readonly INotificationService _notificationService;

        public MinProfilController(
            IBrukerService brukerService,
            ILogger<MinProfilController> logger,
            INotificationService notificationService)
        {
            _brukerService = brukerService;
            _logger = logger;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Login", "Login");
            }

            try
            {
                var bruker = await _brukerService.GetBrukerByEmail(brukerEmail);
                if (bruker == null)
                {
                    _logger.LogWarning("Bruker ikke funnet for epost: {Email}", brukerEmail);
                    return RedirectToAction("Login", "Login");
                }

                return View(bruker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av brukerprofil for {Email}", brukerEmail);
                _notificationService.AddErrorMessage("Kunne ikke hente brukerprofilen din. Prøv igjen senere.");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OppdaterBruker(BrukerRequest brukerRequest)
        {
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                _notificationService.AddErrorMessage("Vennligst sjekk at alle felt er fylt ut korrekt.");
                return View("Index", brukerRequest);
            }

            try
            {
                var oppdatert = await _brukerService.UpdateBruker(brukerRequest);
                if (oppdatert)
                {
                    _notificationService.AddSuccessMessage("Profilen din ble oppdatert!");
                    return RedirectToAction("Index");
                }
                
                _notificationService.AddErrorMessage("Kunne ikke oppdatere profilen din. Prøv igjen.");
                return View("Index", brukerRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av bruker: {@Bruker}", brukerRequest);
                _notificationService.AddErrorMessage("En feil oppstod ved oppdatering av profilen din.");
                return View("Index", brukerRequest);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SlettBruker(int brukerId)
        {
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var slettet = await _brukerService.DeleteBruker(brukerId);
                if (slettet)
                {
                    _notificationService.AddSuccessMessage("Brukeren ble slettet.");
                    return RedirectToAction("Index", "Login");
                }
                
                _notificationService.AddErrorMessage("Kunne ikke slette brukeren. Prøv igjen senere.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved sletting av bruker: {Email}", brukerEmail);
                _notificationService.AddErrorMessage("En feil oppstod ved sletting av profilen din.");
                return RedirectToAction("Index");
            }
        }
    }
}        
    