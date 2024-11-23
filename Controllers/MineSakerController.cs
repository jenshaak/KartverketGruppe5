using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Models.RequestModels;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Saksbehandler,Admin")]
    public class MineSakerController : Controller
    {
        private readonly InnmeldingService _innmeldingService;
        private readonly LokasjonService _lokasjonService;
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly FylkeService _fylkeService;
        private readonly ILogger<MineSakerController> _logger;

        public MineSakerController(InnmeldingService innmeldingService, LokasjonService lokasjonService, FylkeService fylkeService, SaksbehandlerService saksbehandlerService, ILogger<MineSakerController> logger)
        {
            _innmeldingService = innmeldingService;
            _lokasjonService = lokasjonService;
            _saksbehandlerService = saksbehandlerService;
            _fylkeService = fylkeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string sortOrder = "date_desc", 
            string statusFilter = "", 
            string fylkeFilter = "", 
            string kommuneFilter = "",
            int page = 1)
        {
            try 
            {
                _logger.LogInformation(
                    "Henter saker med sortOrder: {SortOrder}, statusFilter: {StatusFilter}, fylkeFilter: {FylkeFilter}, page: {Page}", 
                    sortOrder, statusFilter, fylkeFilter, page);

                SetupViewData(sortOrder, statusFilter, fylkeFilter, kommuneFilter);
                ViewData["ActionName"] = "Behandle";

                var saksbehandlerId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");
                if (saksbehandlerId == 0)
                {
                    _logger.LogWarning("Ingen gyldig SaksbehandlerId funnet i claims");
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                
                var request = new InnmeldingRequest
                {
                    SaksbehandlerId = saksbehandlerId,
                    SortOrder = sortOrder,
                    StatusFilter = statusFilter,
                    FylkeFilter = fylkeFilter,
                    KommuneFilter = kommuneFilter,
                    Page = page
                };

                _logger.LogInformation("Henter innmeldinger for saksbehandler {SaksbehandlerId}", saksbehandlerId);
                var result = await _innmeldingService.GetInnmeldinger(request);
                
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av saker for saksbehandler");
                return View(new PagedResult<InnmeldingViewModel> { Items = new List<InnmeldingViewModel>() });
            }
        }

        private void SetupViewData(string sortOrder, string statusFilter, string fylkeFilter, string kommuneFilter)
        {
            ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentFylke"] = fylkeFilter;
            ViewData["CurrentKommune"] = Request.Query["kommuneFilter"].ToString();
            ViewData["Statuses"] = _innmeldingService.GetAllStatuses();
        }

        public async Task<IActionResult> Behandle(int id)
        {
            try
            {
                var innmelding = await _innmeldingService.GetInnmeldingById(id);
                if (innmelding == null)
                {
                    return NotFound();
                }

                var lokasjon = await _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
                if (lokasjon == null)
                {
                    return NotFound();
                }

                var saksbehandlere = await _saksbehandlerService.GetAllSaksbehandlere();

                ViewBag.Lokasjon = lokasjon;
                ViewBag.Saksbehandlere = saksbehandlere;

                return View(innmelding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmelding {id}", id);
                return StatusCode(500);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> FullforBehandling(int innmeldingId, string kommentar, string status)
        {
            try 
            {
                await _innmeldingService.UpdateStatusAndKommentar(innmeldingId, kommentar, status);
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding {innmeldingId}", innmeldingId);
                TempData["Error"] = "Det oppstod en feil ved behandling av saken.";
                return RedirectToAction("Index");
            }
        }
    }
}
