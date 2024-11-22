using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Bruker")]
    public class MapChangeController : Controller
    {
        private readonly LokasjonService _lokasjonService;
        private readonly InnmeldingService _innmeldingService;
        private readonly BildeService _bildeService;
        private readonly ILogger<MapChangeController> _logger;

        public MapChangeController(
            LokasjonService lokasjonService, 
            InnmeldingService innmeldingService,
            BildeService bildeService,
            ILogger<MapChangeController> logger)
        {
            _lokasjonService = lokasjonService ?? throw new ArgumentNullException(nameof(lokasjonService));
            _innmeldingService = innmeldingService ?? throw new ArgumentNullException(nameof(innmeldingService));
            _bildeService = bildeService ?? throw new ArgumentNullException(nameof(bildeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            _logger.LogInformation("Starting Index POST method with Latitude: {Latitude}, Longitude: {Longitude}, GeoJson length: {GeoJsonLength}, Beskrivelse length: {BeskrivelseLength}, GeometriType: {GeometriType}",
                model.Latitude, model.Longitude, 
                model.GeoJson?.Length ?? 0, 
                beskrivelse?.Length ?? 0,
                model.GeometriType ?? "null");

            if (string.IsNullOrEmpty(model.GeometriType))
            {
                model.GeometriType = "Point";
                _logger.LogInformation("Setting default GeometriType to Point");
            }

            if (string.IsNullOrEmpty(beskrivelse))
            {
                _logger.LogWarning("Beskrivelse mangler");
                ModelState.AddModelError("", "Beskrivelse er påkrevd");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            var brukerId = HttpContext.Session.GetInt32("BrukerId");
            _logger.LogInformation("BrukerId from session: {BrukerId}", brukerId);
            
            if (brukerId == null)
            {
                _logger.LogWarning("No BrukerId found in session");
                return RedirectToAction("Index", "Login");
            }

            try
            {
                if (string.IsNullOrEmpty(model.GeoJson))
                {
                    _logger.LogWarning("GeoJson data is missing");
                    ModelState.AddModelError("", "GeoJson data mangler");
                    return View(model);
                }

                _logger.LogInformation("Adding lokasjon to database");
                int lokasjonId = await _lokasjonService.AddLokasjon(
                    model.GeoJson,
                    model.Latitude,
                    model.Longitude,
                    model.GeometriType
                );
                _logger.LogInformation("Lokasjon added with ID: {LokasjonId}", lokasjonId);

                if (lokasjonId <= 0)
                {
                    _logger.LogError("Failed to save lokasjon, returned ID: {LokasjonId}", lokasjonId);
                    throw new Exception("Feil ved lagring av lokasjon");
                }

                _logger.LogInformation("Getting kommune ID for coordinates: {Latitude}, {Longitude}", 
                    model.Latitude, model.Longitude);
                int kommuneId = await _lokasjonService.GetKommuneIdFromCoordinates(model.Latitude, model.Longitude);
                _logger.LogInformation("Got kommune ID: {KommuneId}", kommuneId);

                _logger.LogInformation("Adding innmelding to database");
                var innmeldingId = _innmeldingService.AddInnmelding(
                    brukerId: brukerId.Value,
                    kommuneId: kommuneId,
                    lokasjonId: lokasjonId,
                    beskrivelse: beskrivelse,
                    bildeSti: null
                );
                _logger.LogInformation("Innmelding added with ID: {InnmeldingId}", innmeldingId);

                if (innmeldingId <= 0)
                {
                    _logger.LogError("Failed to save innmelding, returned ID: {InnmeldingId}", innmeldingId);
                    throw new Exception("Feil ved lagring av innmelding");
                }

                if (bilde != null)
                {
                    var bildeSti = await _bildeService.LagreBilde(bilde, innmeldingId);
                    if (!string.IsNullOrEmpty(bildeSti))
                    {
                        await _innmeldingService.UpdateBildeSti(innmeldingId, bildeSti);
                    }
                }

                _logger.LogInformation("Successfully saved both lokasjon and innmelding. Redirecting to MineInnmeldinger");
                TempData["Success"] = "Innmelding er lagret";
                return RedirectToAction("Index", "MineInnmeldinger");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during save operation: {ErrorMessage}", ex.Message);
                ModelState.AddModelError("", "Kunne ikke lagre innmeldingen: " + ex.Message);
                return View(model);
            }
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