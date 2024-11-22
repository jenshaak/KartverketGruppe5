using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using KartverketGruppe5.Models.RequestModels;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Saksbehandler,Admin")] 
    public class SaksbehandlingController : Controller
    {
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly InnmeldingService _innmeldingService;
        private readonly LokasjonService _lokasjonService;
        private readonly KommuneService _kommuneService;
        private readonly FylkeService _fylkeService;
        private readonly ILogger<SaksbehandlingController> _logger;

        public SaksbehandlingController(
            SaksbehandlerService saksbehandlerService, 
            InnmeldingService innmeldingService, 
            LokasjonService lokasjonService, 
            KommuneService kommuneService, 
            FylkeService fylkeService, 
            ILogger<SaksbehandlingController> logger)
        {
            _saksbehandlerService = saksbehandlerService ?? throw new ArgumentNullException(nameof(saksbehandlerService));
            _innmeldingService = innmeldingService ?? throw new ArgumentNullException(nameof(innmeldingService));
            _lokasjonService = lokasjonService ?? throw new ArgumentNullException(nameof(lokasjonService));
            _kommuneService = kommuneService ?? throw new ArgumentNullException(nameof(kommuneService));
            _fylkeService = fylkeService ?? throw new ArgumentNullException(nameof(fylkeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(
            string sortOrder = "date_desc", 
            string statusFilter = "",
            string fylkeFilter = "",
            int page = 1)
        {
            try 
            {
                SetupViewData(sortOrder, statusFilter, fylkeFilter);
                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                
                var request = new InnmeldingRequest
                {
                    SortOrder = sortOrder,
                    StatusFilter = statusFilter,
                    FylkeFilter = fylkeFilter,
                    Page = page
                };

                return View(await _innmeldingService.GetInnmeldinger(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmeldinger");
                return View(new PagedResult<InnmeldingViewModel> { Items = new List<InnmeldingViewModel>() });
            }
        }

        private void SetupViewData(string sortOrder, string statusFilter, string fylkeFilter)
        {
            ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentFylke"] = fylkeFilter;
            ViewData["Statuses"] = _innmeldingService.GetAllStatuses();
        }

        [HttpGet]
        public async Task<IActionResult> SearchKommuner(string term)
        {
            try
            {
                var kommuner = await _kommuneService.SearchKommuner(term);
                return Json(kommuner.Select(k => new { id = k.KommuneId, text = k.Navn }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved søk etter kommuner");
                return Json(new List<object>());
            }
        }

        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> Detaljer(int id)
        {
            try
            {
                var innmelding = await _innmeldingService.GetInnmeldingById(id);
                if (innmelding == null)
                {
                    return NotFound();
                }

                var lokasjon = await _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
                var kommune = await _kommuneService.GetKommuneById(innmelding.KommuneId);
                if (lokasjon == null || kommune == null)
                {
                    return NotFound();
                }

                var saksbehandlerResult = await _saksbehandlerService.GetAllSaksbehandlere();
                ViewBag.Saksbehandlere = saksbehandlerResult.Items.ToList();
                ViewBag.Lokasjon = lokasjon;

                var viewModel = new InnmeldingViewModel
                {
                    InnmeldingId = innmelding.InnmeldingId,
                    BrukerId = innmelding.BrukerId,
                    KommuneId = innmelding.KommuneId,
                    LokasjonId = innmelding.LokasjonId,
                    Beskrivelse = innmelding.Beskrivelse,
                    Status = innmelding.Status,
                    BildeSti = innmelding.BildeSti,
                    OpprettetDato = innmelding.OpprettetDato,
                    KommuneNavn = kommune.Navn,
                    SaksbehandlerId = innmelding.SaksbehandlerId,
                    SaksbehandlerNavn = innmelding.SaksbehandlerNavn,
                    StatusClass = GetStatusClass(innmelding.Status)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av detaljer for innmelding {id}", id);
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Videresend(int innmeldingId, int saksbehandlerId)
        {
            try
            {
                await _innmeldingService.UpdateInnmeldingSaksbehandler(innmeldingId, saksbehandlerId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved videresending av innmelding {innmeldingId}", innmeldingId);
                return RedirectToAction("Index");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Behandle(int id)
        {
            try 
            {
                var innmelding = await _innmeldingService.GetInnmeldingById(id);
                if (innmelding == null)
                {
                    return NotFound();
                }

                var currentUserId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");
                if (currentUserId == 0)
                {
                    return Forbid();
                }

                await _innmeldingService.UpdateInnmeldingStatus(id, "Under behandling", currentUserId);
                return RedirectToAction("Index", "MineSaker");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved behandling av innmelding {id}", id);
                return RedirectToAction("Index");
            }
        }

        private static string GetStatusClass(string status) => status switch
        {
            "Ny" => "bg-blue-100 text-blue-800",
            "Under behandling" => "bg-yellow-100 text-yellow-800",
            "Fullført" => "bg-green-100 text-green-800",
            "Avvist" => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
}
