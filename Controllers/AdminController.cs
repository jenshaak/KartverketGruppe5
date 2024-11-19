using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
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

        public async Task<IActionResult> Index(string sortOrder = "date_desc", int page = 1)
        {
            ViewData["AdminSortParam"] = sortOrder == "admin_desc" ? "admin_asc" : "admin_desc";
            ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentPage"] = page;

            var result = await _saksbehandlerService.GetAllSaksbehandlere(sortOrder, page);
            return View(result);
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

        [HttpGet]
        public async Task<IActionResult> Rediger(int id)
        {
            var saksbehandler = await _saksbehandlerService.GetSaksbehandlerById(id);
            if (saksbehandler == null)
            {
                return NotFound();
            }

            // Konverter Saksbehandler til ViewModel
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