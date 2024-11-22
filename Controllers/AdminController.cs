using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Authorization;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Admin")]
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

        [ValidateAntiForgeryToken]
        public IActionResult Register()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            return View();
        }

        public async Task<IActionResult> Index(
            string sortOrder = PagedResult<Saksbehandler>.DefaultSortOrder, 
            int page = PagedResult<Saksbehandler>.DefaultPage)
        {
            try 
            {
                SetSortingViewData(sortOrder, page);
                var result = await _saksbehandlerService.GetAllSaksbehandlere(sortOrder, page);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av saksbehandlere");
                TempData["Error"] = "Det oppstod en feil ved henting av saksbehandlere.";
                return RedirectToAction("Index", "Home");
            }
        }

        private void SetSortingViewData(string sortOrder, int page)
        {
            ViewData["AdminSortParam"] = sortOrder == "admin_desc" ? "admin_asc" : "admin_desc";
            ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentPage"] = page;
        }

        [ValidateAntiForgeryToken]
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


        [ValidateAntiForgeryToken]
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

        [ValidateAntiForgeryToken]
        [HttpGet]
        public async Task<IActionResult> Rediger(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var saksbehandler = await _saksbehandlerService.GetSaksbehandlerById(id);
                if (saksbehandler == null)
                {
                    return NotFound();
                }

                var viewModel = new SaksbehandlerRegistrerViewModel
                {
                    SaksbehandlerId = saksbehandler.SaksbehandlerId,
                    Fornavn = saksbehandler.Fornavn,
                    Etternavn = saksbehandler.Etternavn,
                    Email = saksbehandler.Email,
                    Admin = saksbehandler.Admin,
                    OpprettetDato = saksbehandler.OpprettetDato
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Feil ved henting av saksbehandler: {ex.Message}");
                TempData["Error"] = "Det oppstod en feil ved henting av saksbehandler.";
                return RedirectToAction("Index");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
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
                    TempData["Success"] = "Saksbehandler oppdatert!";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Kunne ikke oppdatere saksbehandler");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating saksbehandler: {ex.Message}");
                ModelState.AddModelError("", "En feil oppstod ved oppdatering");
                return View(viewModel);
            }
        }
    }
} 