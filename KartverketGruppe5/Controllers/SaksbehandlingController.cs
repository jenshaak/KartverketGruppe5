using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.Helpers;

namespace KartverketGruppe5.Controllers
{
    /// <summary>
    /// Controller for saksbehandling
    /// </summary>
    [Authorize(Roles = "Saksbehandler,Admin")] 
    public class SaksbehandlingController : BaseController
    {
        private const string DefaultSortOrder = "date_desc";
        private const int DefaultPage = 1;
        private const string UnderBehandlingStatus = "Under behandling";

        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly ILokasjonService _lokasjonService;
        private readonly IKommuneService _kommuneService;
        private readonly IFylkeService _fylkeService;
        public SaksbehandlingController(
            ISaksbehandlerService saksbehandlerService, 
            IInnmeldingService innmeldingService, 
            ILokasjonService lokasjonService, 
            IKommuneService kommuneService, 
            IFylkeService fylkeService, 
            ILogger<SaksbehandlingController> logger,
            INotificationService notificationService)
            : base(innmeldingService, logger, notificationService)
        {
            _saksbehandlerService = saksbehandlerService ?? throw new ArgumentNullException(nameof(saksbehandlerService));
            _lokasjonService = lokasjonService ?? throw new ArgumentNullException(nameof(lokasjonService));
            _kommuneService = kommuneService ?? throw new ArgumentNullException(nameof(kommuneService));
            _fylkeService = fylkeService ?? throw new ArgumentNullException(nameof(fylkeService));
        }

        /// <summary>
        /// Viser saksbehandlingssiden
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string sortOrder = DefaultSortOrder, 
            string statusFilter = "",
            string fylkeFilter = "",
            string kommuneFilter = "",
            int page = DefaultPage)
        {
            try 
            {
                SetupViewData(sortOrder, statusFilter, fylkeFilter, kommuneFilter);
                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                var innmeldinger = await GetInnmeldinger(null, sortOrder, statusFilter, fylkeFilter, kommuneFilter, page, false);
                return View(innmeldinger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmeldinger");
                return View(CreateEmptyPagedResult());
            }
        }

        /// <summary>
        /// Søker etter kommuner
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchKommuner(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            if (term.Length < 1)
            {
                return Json(new List<object>());
            }

            if (term.Length > 100)
            {
                term = term.Substring(0, 100);
            }

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

        /// <summary>
        /// Viser detaljer for en innmelding
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detaljer(int id)
        {
            try
            {
                var (innmelding, lokasjon, saksbehandlere) = await GetDetaljerData(id);
                if (innmelding == null || lokasjon == null)
                {
                    return NotFound();
                }

                ViewBag.Saksbehandlere = saksbehandlere;
                ViewBag.Lokasjon = lokasjon;

                return View(innmelding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av detaljer for innmelding {InnmeldingId}", id);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Videresender en innmelding
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Videresend(int innmeldingId, int saksbehandlerId)
        {
            try
            {
                await _innmeldingService.UpdateInnmeldingStatus(innmeldingId, saksbehandlerId, UnderBehandlingStatus);
                _notificationService.AddSuccessMessage("Innmelding videresendt");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved videresending av innmelding {innmeldingId}", innmeldingId);
                _notificationService.AddErrorMessage("Feil ved videresending av innmelding");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Behandler en innmelding
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Behandle(int id)
        {
            try 
            {
                var saksbehandlerId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");
                if (saksbehandlerId == 0)
                {
                    return Forbid();
                }

                var innmelding = await _innmeldingService.GetInnmeldingById(id);
                if (innmelding == null)
                {
                    return NotFound();
                }

                await _innmeldingService.UpdateInnmeldingStatus(id, saksbehandlerId, UnderBehandlingStatus);
                _notificationService.AddSuccessMessage("Du har blitt ansvarlig for innmeldingen");
                return RedirectToAction("Index", "MineSaker");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved overtakelse av innmelding {InnmeldingId}", id);
                _notificationService.AddErrorMessage("Feil ved overtakelse av innmelding");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Hjelpemetode som henter behandlingsdata
        /// </summary>
        private async Task<(InnmeldingViewModel? innmelding, LokasjonViewModel? lokasjon, List<Saksbehandler> saksbehandlere)> 
            GetDetaljerData(int id)
        {
            var innmelding = await _innmeldingService.GetInnmeldingById(id);
            if (innmelding == null)
            {
                return (null, null, new List<Saksbehandler>());
            }

            var lokasjon = await _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
            var kommune = await _kommuneService.GetKommuneById(innmelding.KommuneId);
            if (lokasjon == null || kommune == null)
            {
                return (null, null, new List<Saksbehandler>());
            }

            var saksbehandlerResult = await _saksbehandlerService.GetAllSaksbehandlere();
            return (innmelding, lokasjon, saksbehandlerResult.Items.ToList());
        }
    }
}
