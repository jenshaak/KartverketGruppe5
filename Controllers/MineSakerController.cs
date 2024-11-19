using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
    [Authorize]
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
            int page = 1)
        {
            if (!User.IsInRole("Saksbehandler") && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            ViewData["DateSortParam"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentFylke"] = fylkeFilter;
            ViewData["Statuses"] = _innmeldingService.GetAllStatuses();

            try 
            {
                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                
                var saksbehandlerId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");

                var result = await _innmeldingService.GetInnmeldinger(
                    saksbehandlerId: saksbehandlerId,
                    sortOrder: sortOrder,
                    statusFilter: statusFilter,
                    fylkeFilter: fylkeFilter,
                    page: page);

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmeldinger");
                return View(new PagedResult<InnmeldingModel> { Items = new List<InnmeldingModel>() });
            }
        }

        public async Task<IActionResult> Behandle(int id)
        {
            var innmelding = await _innmeldingService.GetInnmeldingById(id);
            if (innmelding == null)
            {
                return NotFound();
            }

            var lokasjon = _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
            ViewBag.Lokasjon = lokasjon;

            var saksbehandlere = await _saksbehandlerService.GetAllSaksbehandlere();
            ViewBag.Saksbehandlere = saksbehandlere;

            return View(innmelding);
        }

        [HttpPost]
        public async Task<IActionResult> FullforBehandling(int innmeldingId, string kommentar, string status)
        {
            await _innmeldingService.UpdateStatusAndKommentar(innmeldingId, kommentar, status);
            return RedirectToAction("Index");
        }
    }
}
