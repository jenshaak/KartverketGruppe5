using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.Interfaces;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Saksbehandler,Admin")]
    public class MineSakerController : BaseController
    {
        private const string DefaultSortOrder = "date_desc";
        private const int DefaultPage = 1;
        private const string SaksbehandlerIdClaimType = "SaksbehandlerId";

        private readonly ILokasjonService _lokasjonService;
        private readonly ISaksbehandlerService _saksbehandlerService;
        private readonly IFylkeService _fylkeService;
        private readonly INotificationService _notificationService;
        public MineSakerController(
            IInnmeldingService innmeldingService, 
            ILokasjonService lokasjonService, 
            IFylkeService fylkeService, 
            ISaksbehandlerService saksbehandlerService, 
            INotificationService notificationService,
            ILogger<MineSakerController> logger)
            : base(innmeldingService, logger)
        {
            _lokasjonService = lokasjonService;
            _saksbehandlerService = saksbehandlerService;
            _fylkeService = fylkeService;
            _notificationService = notificationService;
        }

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
                ViewData["ActionName"] = "Behandle";

                var saksbehandlerId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");
                if (saksbehandlerId == 0)
                {
                    _logger.LogWarning("Ingen gyldig SaksbehandlerId funnet i claims");
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                var innmeldinger = await GetInnmeldinger(saksbehandlerId, sortOrder, statusFilter, fylkeFilter, kommuneFilter, page, false);
                return View(innmeldinger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av saker for saksbehandler");
                return View(CreateEmptyPagedResult());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Behandle(int id)
        {
            try
            {
                var (innmelding, lokasjon) = await GetBehandlingData(id);
                if (innmelding == null || lokasjon == null)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FullforBehandling(int innmeldingId, string kommentar, string status)
        {
            try 
            {
                await _innmeldingService.UpdateStatusAndKommentar(innmeldingId, kommentar, status);
                _notificationService.AddSuccessMessage("Innmelding fullført");
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding {innmeldingId}", innmeldingId);
                _notificationService.AddErrorMessage("Feil ved behandling av saken.");
                return RedirectToAction("Index");
            }
        }

        // Private hjelpemetode

        private async Task<(InnmeldingViewModel? innmelding, LokasjonViewModel? lokasjon)> GetBehandlingData(int id)
        {
            var innmelding = await _innmeldingService.GetInnmeldingById(id);
            if (innmelding == null)
            {
                return (null, null);
            }

            var lokasjon = await _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
            if (lokasjon == null)
            {
                return (null, null);
            }

            return (innmelding, lokasjon);
        }

    }
}
