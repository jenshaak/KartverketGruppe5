using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace KartverketGruppe5.Controllers
{
    [Authorize]
    public class SaksbehandlingController : Controller
    {
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly InnmeldingService _innmeldingService;
        private readonly LokasjonService _lokasjonService;
        private readonly KommuneService _kommuneService;
        private readonly FylkeService _fylkeService;
        private readonly ILogger<SaksbehandlingController> _logger;

        public SaksbehandlingController(SaksbehandlerService saksbehandlerService, InnmeldingService innmeldingService, LokasjonService lokasjonService, KommuneService kommuneService, FylkeService fylkeService, ILogger<SaksbehandlingController> logger)
        {
            _saksbehandlerService = saksbehandlerService;
            _innmeldingService = innmeldingService;
            _lokasjonService = lokasjonService;
            _kommuneService = kommuneService;
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
            ViewData["Statuses"] = InnmeldingService.GetAllStatuses();

            try 
            {
                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                
                var result = await _innmeldingService.GetInnmeldinger(
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

        [HttpGet]
        public async Task<IActionResult> SearchKommuner(string term)
        {
            var kommuner = await _kommuneService.SearchKommuner(term);
            return Json(kommuner.Select(k => new { id = k.KommuneId, text = k.Navn }));
        }

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
            var innmelding = await _innmeldingService.GetInnmeldingById(id);
            if (innmelding == null)
            {
                return NotFound();
            }

            var lokasjon = _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
            if (lokasjon == null)
            {
                return NotFound();
            }

            var kommune = _kommuneService.GetKommuneById(innmelding.KommuneId);
            if (kommune == null)
            {
                return NotFound();
            }

            var saksbehandlerResult = await _saksbehandlerService.GetAllSaksbehandlere();
            ViewBag.Saksbehandlere = saksbehandlerResult.Items.ToList();

            var innmeldingModel = new InnmeldingModel
            {
                InnmeldingId = innmelding.InnmeldingId,
                BrukerId = innmelding.BrukerId,
                KommuneId = innmelding.KommuneId,
                LokasjonId = innmelding.LokasjonId,
                Beskrivelse = innmelding.Beskrivelse,
                Status = innmelding.Status,
                OpprettetDato = innmelding.OpprettetDato,
                KommuneNavn = kommune.Navn,
                SaksbehandlerId = innmelding.SaksbehandlerId,
                SaksbehandlerNavn = innmelding.SaksbehandlerNavn,
                StatusClass = GetStatusClass(innmelding.Status)
            };

            ViewBag.Lokasjon = lokasjon;
            return View(innmeldingModel);
        }

        public async Task<IActionResult> Videresend(int innmeldingId, int saksbehandlerId)
        {
            await _innmeldingService.UpdateInnmeldingSaksbehandler(innmeldingId, saksbehandlerId);
            return RedirectToAction("Index", "Saksbehandling");
        }

        [HttpPost]
        public async Task<IActionResult> Behandle(int id)
        {
            var innmeldingModel = await _innmeldingService.GetInnmeldingById(id);
            if (innmeldingModel == null)
            {
                return NotFound();
            }

            var currentUserId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");
            
            // Konverter til Innmelding
            var innmelding = new Innmelding
            {
                InnmeldingId = innmeldingModel.InnmeldingId,
                BrukerId = innmeldingModel.BrukerId,
                KommuneId = innmeldingModel.KommuneId,
                LokasjonId = innmeldingModel.LokasjonId,
                Beskrivelse = innmeldingModel.Beskrivelse,
                Status = "Under behandling",
                SaksbehandlerId = currentUserId,
                OpprettetDato = innmeldingModel.OpprettetDato
            };

            await _innmeldingService.UpdateInnmeldingStatus(innmelding.InnmeldingId, "Under behandling", currentUserId);
            return RedirectToAction("Index", "MineSaker");
        }

        private string GetStatusClass(string status) => status switch
        {
            "Ny" => "bg-blue-100 text-blue-800",
            "Under behandling" => "bg-yellow-100 text-yellow-800",
            "Fullført" => "bg-green-100 text-green-800",
            "Avvist" => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
}
