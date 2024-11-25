using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Services.Interfaces;
namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Bruker")]
    public class MapChangeController : Controller
    {
        private const string BrukerIdSessionKey = "BrukerId";
        private readonly ILokasjonService _lokasjonService;
        private readonly IInnmeldingService _innmeldingService;
        private readonly IBildeService _bildeService;
        private readonly ILogger<MapChangeController> _logger;
        private readonly INotificationService _notificationService;

        public MapChangeController(
            ILokasjonService lokasjonService, 
            IInnmeldingService innmeldingService,
            IBildeService bildeService,
            INotificationService notificationService,
            ILogger<MapChangeController> logger)
        {
            _lokasjonService = lokasjonService ?? throw new ArgumentNullException(nameof(lokasjonService));
            _innmeldingService = innmeldingService ?? throw new ArgumentNullException(nameof(innmeldingService));
            _bildeService = bildeService ?? throw new ArgumentNullException(nameof(bildeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public IActionResult Index()
        {
            var brukerId = HttpContext.Session.GetInt32("BrukerId");
            if (brukerId == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LokasjonViewModel model, string beskrivelse, IFormFile? bilde)
        {
            try
            {
                LogInputParameters(model, beskrivelse);

                if (!ValidateInput(model, beskrivelse))
                {
                    return View(model);
                }

                var brukerId = GetBrukerIdFromSession();
                if (!brukerId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                return await ProcessInnmelding(model, beskrivelse, bilde, brukerId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil under lagring av innmelding");
                ModelState.AddModelError("", "Kunne ikke lagre innmeldingen: " + ex.Message);
                return View(model);
            }
        }

        private async Task<IActionResult> ProcessInnmelding(
            LokasjonViewModel model, string beskrivelse, IFormFile? bilde, int brukerId)
        {
            if(model.GeoJson == null)
            {
                ModelState.AddModelError("", "GeoJson data mangler");
                return View(model);
            }
            try
            {
                var lokasjonId = await _lokasjonService.AddLokasjon(
                    model.GeoJson,
                    model.Latitude,
                    model.Longitude,
                    model.GeometriType
                );

                if (lokasjonId <= 0)
                {
                    throw new Exception("Feil ved lagring av lokasjon");
                }

                var kommuneId = await _lokasjonService.GetKommuneIdFromCoordinates(
                    model.Latitude, model.Longitude);

                var innmeldingId = await SaveInnmelding(brukerId, kommuneId, lokasjonId, beskrivelse);
                
                if (bilde != null)
                {
                    await HandleBildeUpload(bilde, innmeldingId);
                }

                _notificationService.AddSuccessMessage("Innmelding ble sendt inn");
                return RedirectToAction("Index", "MineInnmeldinger");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved lagring av innmelding");
                _notificationService.AddErrorMessage("Kunne ikke lagre innmeldingen: " + ex.Message);
                return View(model);
            }
        }

        private bool ValidateInput(LokasjonViewModel model, string beskrivelse)
        {
            if (string.IsNullOrEmpty(model.GeometriType))
            {
                model.GeometriType = "Point";
            }

            if (string.IsNullOrEmpty(beskrivelse))
            {
                ModelState.AddModelError("", "Beskrivelse er påkrevd");
                return false;
            }

            if (string.IsNullOrEmpty(model.GeoJson))
            {
                ModelState.AddModelError("", "GeoJson data mangler");
                return false;
            }

            return ModelState.IsValid;
        }

        private async Task<int> SaveInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse)
        {
            try
            {
                var innmeldingId = await _innmeldingService.CreateInnmelding(
                    brukerId: brukerId,
                    kommuneId: kommuneId,
                    lokasjonId: lokasjonId,
                    beskrivelse: beskrivelse,
                    bildeSti: null
                );

                if (innmeldingId <= 0)
                {
                    throw new Exception("Feil ved lagring av innmelding");
                }

                return innmeldingId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved lagring av innmelding");
                throw;
            }
        }

        private async Task HandleBildeUpload(IFormFile bilde, int innmeldingId)
        {
            var bildeSti = await _bildeService.LagreBilde(bilde, innmeldingId);
            if (!string.IsNullOrEmpty(bildeSti))
            {
                await _innmeldingService.UpdateBildeSti(innmeldingId, bildeSti);
            }
        }

        private int? GetBrukerIdFromSession()
        {
            return HttpContext.Session.GetInt32(BrukerIdSessionKey);
        }

        private void LogInputParameters(LokasjonViewModel model, string beskrivelse)
        {
            _logger.LogInformation(
                "Starting Index POST method with Latitude: {Latitude}, Longitude: {Longitude}, " +
                "GeoJson length: {GeoJsonLength}, Beskrivelse length: {BeskrivelseLength}, GeometriType: {GeometriType}",
                model.Latitude, model.Longitude, 
                model.GeoJson?.Length ?? 0, 
                beskrivelse?.Length ?? 0,
                model.GeometriType ?? "null");
        }

        public async Task<IActionResult> ViewInnmelding(int id)
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

            ViewBag.Innmelding = innmelding;
            return View(lokasjon);
        }
    }
} 