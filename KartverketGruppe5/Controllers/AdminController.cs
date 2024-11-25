using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Controller for admin-funksjoner
/// </summary>

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly IKommunePopulateService _kommunePopulateService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ISaksbehandlerService saksbehandlerService,
            IKommunePopulateService kommunePopulateService,
            INotificationService notificationService,
            ILogger<AdminController> logger)
        {
            _saksbehandlerService = saksbehandlerService;
            _kommunePopulateService = kommunePopulateService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string sortOrder = PagedResult<Saksbehandler>.DefaultSortOrder, 
            int page = PagedResult<Saksbehandler>.DefaultPage)
        {
            try 
            {
                SetSortingViewData(sortOrder, page);
                return View(await _saksbehandlerService.GetAllSaksbehandlere(sortOrder, page));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av saksbehandlere");
                _notificationService.AddErrorMessage("Det oppstod en feil ved henting av saksbehandlere.");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Setter sorteringsdata for saksbehandlere
        /// </summary>
        private void SetSortingViewData(string sortOrder, int page)
        {
            ViewData["AdminSortParam"] = sortOrder == "admin_desc" ? "admin_asc" : "admin_desc";
            ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentPage"] = page;
        }

        /// <summary>
        /// Viser saksbehandlerregistreringsskjema
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        /// <summary>
        /// Registrerer en ny saksbehandler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Saksbehandler saksbehandler)
        {
            if (!ModelState.IsValid)
            {
                return View(saksbehandler);
            }

            try
            {
                var result = await _saksbehandlerService.CreateSaksbehandler(saksbehandler);
                if (result)
                {
                    _notificationService.AddSuccessMessage("Saksbehandler opprettet!");
                    return RedirectToAction("Index", "Admin");
                }
                
                ModelState.AddModelError("", "Kunne ikke opprette saksbehandler");
                _notificationService.AddErrorMessage("Kunne ikke opprette saksbehandler");
                return View(saksbehandler);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating saksbehandler: {ex.Message}");
                ModelState.AddModelError("", "En feil oppstod ved registrering");
                _notificationService.AddErrorMessage("En feil oppstod ved registrering");
                return View(saksbehandler);
            }
        }

        /// <summary>
        /// Populerer fylker og kommuner
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PopulateFylkerOgKommuner()
        {
            try
            {
                var result = await _kommunePopulateService.PopulateFylkerOgKommuner();
                _notificationService.AddSuccessMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Feil under oppdatering av fylker og kommuner: {ex.Message}");
                _notificationService.AddErrorMessage("Det oppstod en feil under oppdatering av fylker og kommuner.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Viser saksbehandlerredigeringsskjema
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Rediger(int id)
        {
            try
            {
                var saksbehandler = await _saksbehandlerService.GetSaksbehandlerById(id);
                if (saksbehandler == null)
                {
                    return NotFound();
                }

                return View(MapToViewModel(saksbehandler));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av saksbehandler med ID: {Id}", id);
                _notificationService.AddErrorMessage("Det oppstod en feil ved henting av saksbehandler.");
                return RedirectToAction("Index");
            }
        }

        private static SaksbehandlerRegistrerViewModel MapToViewModel(Saksbehandler saksbehandler)
        {
            return new SaksbehandlerRegistrerViewModel
            {
                SaksbehandlerId = saksbehandler.SaksbehandlerId,
                Fornavn = saksbehandler.Fornavn,
                Etternavn = saksbehandler.Etternavn,
                Email = saksbehandler.Email,
                Admin = saksbehandler.Admin,
                OpprettetDato = saksbehandler.OpprettetDato
            };
        }

        /// <summary>
        /// Oppdaterer en saksbehandler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rediger(SaksbehandlerRegistrerViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.Passord))
            {
                ModelState.Remove("Passord");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var result = await _saksbehandlerService.UpdateSaksbehandler(viewModel);
                if (result)
                {
                    _notificationService.AddSuccessMessage("Saksbehandler oppdatert!");
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Kunne ikke oppdatere saksbehandler");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating saksbehandler: {ex.Message}");
                ModelState.AddModelError("", "En feil oppstod ved oppdatering");
                _notificationService.AddErrorMessage("En feil oppstod ved oppdatering");
                return View(viewModel);
            }
        }

        /// <summary>
        /// Sletter en saksbehandler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SlettSaksbehandler(int saksbehandlerId)
        {
            try
            {
                var slettet = await _saksbehandlerService.DeleteSaksbehandler(saksbehandlerId);
                if (slettet)
                {
                    _notificationService.AddSuccessMessage("Saksbehandler ble slettet.");
                    return RedirectToAction("Index");
                }
                
                _notificationService.AddErrorMessage("Kunne ikke slette saksbehandler. Prøv igjen senere.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved sletting av saksbehandler med ID: {SaksbehandlerId}", saksbehandlerId);
                _notificationService.AddErrorMessage("En feil oppstod ved sletting av saksbehandler.");
                return RedirectToAction("Index");
            }
        }
    }
} 