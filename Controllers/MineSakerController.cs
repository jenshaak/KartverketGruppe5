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

        public async Task<IActionResult> Index(string sortOrder = "date_desc", 
            string statusFilter = "", string fylkeFilter = "", int page = 1)
        {
            SetupViewData(sortOrder, statusFilter, fylkeFilter);
            ViewData["ActionName"] = "Behandle";

            try 
            {
                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                
                var saksbehandlerId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");
                var request = new InnmeldingRequest
                {
                    SaksbehandlerId = saksbehandlerId,
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

        public async Task<IActionResult> Behandle(int id)
        {
            try
            {
                _logger.LogInformation($"Henter innmelding med ID {id}");
                var innmelding = await _innmeldingService.GetInnmeldingById(id);
                _logger.LogInformation($"Hentet innmelding med ID {id}: {innmelding.Beskrivelse}");
                var lokasjon = _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
                if (lokasjon == null)
                {
                    _logger.LogError($"Fant ikke lokasjon med ID {innmelding.LokasjonId} for innmelding {id}");
                    return NotFound();
                }

                var saksbehandlere = await _saksbehandlerService.GetAllSaksbehandlere();
                if (!saksbehandlere.Items.Any())
                {
                    _logger.LogWarning("Ingen saksbehandlere funnet");
                }

                ViewBag.Lokasjon = lokasjon;
                ViewBag.Saksbehandlere = saksbehandlere;

                return View(innmelding);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning($"Fant ikke innmelding med ID {id}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Feil ved henting av innmelding {id}");
                return StatusCode(500);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> FullforBehandling(int innmeldingId, string kommentar, string status)
        {
            await _innmeldingService.UpdateStatusAndKommentar(innmeldingId, kommentar, status);
            return RedirectToAction("Index");
        }
    }
}
