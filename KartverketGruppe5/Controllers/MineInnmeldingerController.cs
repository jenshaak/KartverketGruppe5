using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.Helpers;

namespace KartverketGruppe5.Controllers
{
    /// <summary>
    /// Controller for innmeldinger til brukere
    /// </summary>
    [Authorize(Roles = "Bruker")]
    public class MineInnmeldingerController : BaseController
    {
        private const string BrukerIdSessionKey = "BrukerId";
        private const string DefaultSortOrder = "date_desc";
        private const int DefaultPage = 1;

        private readonly ILokasjonService _lokasjonService;
        private readonly IKommuneService _kommuneService;        
        private readonly IFylkeService _fylkeService;

        public MineInnmeldingerController(
            IInnmeldingService innmeldingService, 
            ILokasjonService lokasjonService, 
            IKommuneService kommuneService, 
            IFylkeService fylkeService, 
            INotificationService notificationService,
            ILogger<MineInnmeldingerController> logger)
            : base(innmeldingService, logger, notificationService)
        {
            _lokasjonService = lokasjonService ?? throw new ArgumentNullException(nameof(lokasjonService));
            _kommuneService = kommuneService ?? throw new ArgumentNullException(nameof(kommuneService));
            _fylkeService = fylkeService ?? throw new ArgumentNullException(nameof(fylkeService));
        }

        /// <summary>
        /// Viser mine innmeldinger
        /// </summary>
        public async Task<IActionResult> Index(
            string sortOrder = DefaultSortOrder, 
            string statusFilter = "",
            string fylkeFilter = "",
            string kommuneFilter = "",
            int page = DefaultPage)
        {
            try 
            {
                var brukerId = GetBrukerIdFromSession();
                if (!brukerId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                SetupViewData(sortOrder, statusFilter, fylkeFilter, kommuneFilter);

                return View(await GetInnmeldinger(brukerId.Value, sortOrder, statusFilter, fylkeFilter, kommuneFilter, page, true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmeldinger");
                var emptyResult = CreateEmptyPagedResult();
                return View(emptyResult);
            }
        }

        /// <summary>
        /// Viser innmelding
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detaljer(int id)
        {
            try
            {
                var brukerId = GetBrukerIdFromSession();
                if (!brukerId.HasValue)
                {
                    _logger.LogWarning("Ingen bruker-ID funnet i session ved forsøk på å se detaljer for innmelding {InnmeldingId}", id);
                    return RedirectToAction("Index", "Login");
                }

                var (innmelding, lokasjon) = await ValidateAndGetInnmeldingDetails(id, brukerId.Value);
                if (innmelding == null || lokasjon == null)
                {
                    return NotFound();
                }

                ViewBag.Lokasjon = lokasjon;
                return View(innmelding);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Uautorisert tilgang forsøkt for innmelding {InnmeldingId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil ved henting av detaljer for innmelding {InnmeldingId}", id);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Sletter innmelding
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SlettInnmelding(int innmeldingId)
        {
            try
            {
                await _innmeldingService.SlettInnmelding(innmeldingId);
                _notificationService.AddSuccessMessage("Innmelding slettet");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved sletting av innmelding {InnmeldingId}", innmeldingId);
                _notificationService.AddErrorMessage("Feil ved sletting av innmelding");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Endrer innmelding
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndreInnmelding(InnmeldingViewModel innmeldingModel, IFormFile? bilde)
        {
            // Kjører validering først
            if (!ValidateInput(innmeldingModel.Beskrivelse) || !ModelState.IsValid)
            {
                _notificationService.AddErrorMessage("Feil ved validering av innmelding");
                return View(innmeldingModel);
            }

            try 
            {
                _logger.LogInformation("Starter endring av innmelding {InnmeldingId}. ModelBrukerId: {ModelBrukerId}", 
                    innmeldingModel.InnmeldingId, innmeldingModel.BrukerId);

                // Henter den originale innmeldingen først
                var originalInnmelding = await _innmeldingService.GetInnmeldingById(innmeldingModel.InnmeldingId);
                if (originalInnmelding == null)
                {
                    _logger.LogWarning("Fant ikke innmelding med id {InnmeldingId}", innmeldingModel.InnmeldingId);
                    return NotFound();
                }

                _logger.LogInformation("LokasjonId: {LokasjonId}", originalInnmelding.LokasjonId);

                var brukerId = HttpContext.Session.GetInt32("BrukerId");
                _logger.LogInformation("Session BrukerId: {SessionBrukerId}, Original BrukerId: {OriginalBrukerId}", 
                    brukerId, originalInnmelding.BrukerId);

                // Sjekk mot original innmelding istedenfor modellen
                if (brukerId != originalInnmelding.BrukerId)
                {
                    _logger.LogWarning("Bruker {BrukerId} forsøkte å endre innmelding {InnmeldingId} som tilhører bruker {OriginalBrukerId}", 
                        brukerId, innmeldingModel.InnmeldingId, originalInnmelding.BrukerId);
                    return Forbid();
                }

                // Sett BrukerId fra originalen
                innmeldingModel.BrukerId = originalInnmelding.BrukerId;

                // Håndterer lokasjon
                LokasjonViewModel? lokasjon = GetLokasjonFromRequest();
                _logger.LogInformation("Lokasjon: {Lokasjon}", lokasjon);
                if (lokasjon != null)
                {
                    _logger.LogInformation("Oppdaterer lokasjon for innmelding {InnmeldingId}: Lat: {Latitude}, Lon: {Longitude}", 
                        innmeldingModel.InnmeldingId, lokasjon.Latitude, lokasjon.Longitude);
                }

                await _innmeldingService.UpdateInnmeldingDetails(innmeldingModel, lokasjon, bilde);
                _notificationService.AddSuccessMessage("Innmelding oppdatert");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding {InnmeldingId}", innmeldingModel.InnmeldingId);
                _notificationService.AddErrorMessage("Feil ved oppdatering av innmelding");
                return RedirectToAction("Detaljer", new { id = innmeldingModel.InnmeldingId });
            }
        }

        /// <summary>
        /// Søker etter kommuner
        /// </summary>
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

        /// <summary>
        /// Henter lokasjon fra request
        /// </summary>
        private LokasjonViewModel? GetLokasjonFromRequest()
        {
            if (!Request.Form.TryGetValue("geoJsonInput", out var geoJson) || 
                !Request.Form.TryGetValue("geometriType", out var geometriType) || 
                !Request.Form.TryGetValue("latitude", out var latitude) || 
                !Request.Form.TryGetValue("longitude", out var longitude) ||
                string.IsNullOrWhiteSpace(latitude.ToString()) ||
                string.IsNullOrWhiteSpace(longitude.ToString()))
            {
                return null;
            }

            if (!Request.Form.TryGetValue("lokasjonId", out var lokasjonId))
            {
                _logger.LogWarning("LokasjonId mangler i form data");
                return null;
            }

            return new LokasjonViewModel
            {
                LokasjonId = int.Parse(lokasjonId.ToString()),
                GeoJson = geoJson.ToString(),
                GeometriType = geometriType.ToString(),
                Latitude = double.Parse(latitude.ToString()),
                Longitude = double.Parse(longitude.ToString())
            };
        }

        /// <summary>
        /// Henter bruker-ID fra session
        /// </summary>
        private int? GetBrukerIdFromSession()
        {
            var brukerId = HttpContext.Session.GetInt32(BrukerIdSessionKey);
            if (brukerId.HasValue)
            {
                _logger.LogInformation("Hentet bruker-ID {BrukerId} fra session", brukerId.Value);
            }
            else
            {
                _logger.LogWarning("Ingen bruker-ID funnet i session");
            }
            return brukerId;
        }

        /// <summary>
        /// Validerer og henter innmelding-detaljer
        /// </summary>
        private async Task<(InnmeldingViewModel? innmelding, LokasjonViewModel? lokasjon)> ValidateAndGetInnmeldingDetails(int id, int brukerId)
        {
            try
            {
                var innmelding = await _innmeldingService.GetInnmeldingById(id);
                if (innmelding == null || innmelding.BrukerId != brukerId)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av detaljer for innmelding {InnmeldingId}", id);
                return (null, null);
            }
        }
    }
} 