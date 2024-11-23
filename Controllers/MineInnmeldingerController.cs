using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using KartverketGruppe5.Models.RequestModels;
using KartverketGruppe5.Services.Interfaces;
namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Bruker")]
    public class MineInnmeldingerController : Controller
    {
        private readonly IInnmeldingService _innmeldingService;
        private readonly ILokasjonService _lokasjonService;
        private readonly IKommuneService _kommuneService;        
        private readonly IFylkeService _fylkeService;
        private readonly ILogger<MineInnmeldingerController> _logger;

        public MineInnmeldingerController(
            IInnmeldingService innmeldingService, 
            ILokasjonService lokasjonService, 
            IKommuneService kommuneService, 
            IFylkeService fylkeService, 
            ILogger<MineInnmeldingerController> logger)
        {
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
            string kommuneFilter = "",
            int page = 1)
        {
            try 
            {
                _logger.LogInformation(
                    "Henter innmeldinger med sortOrder: {SortOrder}, statusFilter: {StatusFilter}, fylkeFilter: {FylkeFilter}, page: {Page}", 
                    sortOrder, statusFilter, fylkeFilter, page);

                SetupViewData(sortOrder, statusFilter, fylkeFilter, kommuneFilter);

                var brukerId = HttpContext.Session.GetInt32("BrukerId");
                if (brukerId == null)
                {
                    _logger.LogWarning("Ingen BrukerId funnet i session");
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                
                var request = new InnmeldingRequest
                {
                    InnmelderBrukerId = brukerId.Value,
                    SortOrder = sortOrder,
                    StatusFilter = statusFilter,
                    FylkeFilter = fylkeFilter,
                    Page = page
                };

                _logger.LogInformation("Henter innmeldinger for bruker {BrukerId}", brukerId.Value);
                return View(await _innmeldingService.GetInnmeldinger(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av innmeldinger for bruker {BrukerId}", 
                    HttpContext.Session.GetInt32("BrukerId"));
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
            ViewData["Statuses"] = new List<string> 
            { 
                "Ny",
                "Under behandling",
                "Godkjent",
                "Avvist"
            };
        }

        [HttpGet]
        public async Task<IActionResult> Detaljer(int id)
        {
            try
            {
                _logger.LogInformation("Henter detaljer for innmelding {InnmeldingId}", id);

                var innmelding = await _innmeldingService.GetInnmeldingById(id);
                if (innmelding == null)
                {
                    _logger.LogWarning("Innmelding med id {InnmeldingId} ikke funnet", id);
                    return NotFound();
                }

                // Sjekk at innmeldingen tilhører innlogget bruker
                var brukerId = HttpContext.Session.GetInt32("BrukerId");
                if (brukerId != innmelding.BrukerId)
                {
                    _logger.LogWarning("Bruker {BrukerId} forsøkte å se innmelding {InnmeldingId} som tilhører en annen bruker", 
                        brukerId, id);
                    return Forbid();
                }

                var lokasjon = await _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
                if (lokasjon == null)
                {
                    _logger.LogError("Lokasjon med id {LokasjonId} ikke funnet for innmelding {InnmeldingId}", 
                        innmelding.LokasjonId, id);
                    return NotFound();
                }

                var kommune = await _kommuneService.GetKommuneById(innmelding.KommuneId);
                if (kommune == null)
                {
                    _logger.LogError("Kommune med id {KommuneId} ikke funnet for innmelding {InnmeldingId}", 
                        innmelding.KommuneId, id);
                    return NotFound();
                }

                var innmeldingViewModel = new InnmeldingViewModel
                {
                    InnmeldingId = innmelding.InnmeldingId,
                    BrukerId = innmelding.BrukerId,
                    KommuneId = innmelding.KommuneId,
                    LokasjonId = innmelding.LokasjonId,
                    Beskrivelse = innmelding.Beskrivelse,
                    Kommentar = innmelding.Kommentar,
                    Status = innmelding.Status,
                    OpprettetDato = innmelding.OpprettetDato,
                    KommuneNavn = kommune.Navn,
                    StatusClass = GetStatusClass(innmelding.Status),
                    BildeSti = innmelding.BildeSti
                };

                ViewBag.Lokasjon = lokasjon;

                _logger.LogInformation("Returnerer detaljer for innmelding {InnmeldingId}", id);
                return View(innmeldingViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av detaljer for innmelding {InnmeldingId}", id);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SlettInnmelding(int id)
        {
            await _innmeldingService.SlettInnmelding(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndreInnmelding(InnmeldingViewModel innmeldingModel, IFormFile? bilde)
        {
            try 
            {
                _logger.LogInformation("Starter endring av innmelding {InnmeldingId}. ModelBrukerId: {ModelBrukerId}", 
                    innmeldingModel.InnmeldingId, innmeldingModel.BrukerId);

                // Hent den originale innmeldingen først
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

                // Håndter lokasjon
                LokasjonViewModel? lokasjon = GetLokasjonFromRequest();
                _logger.LogInformation("Lokasjon: {Lokasjon}", lokasjon);
                if (lokasjon != null)
                {
                    _logger.LogInformation("Oppdaterer lokasjon for innmelding {InnmeldingId}: Lat: {Latitude}, Lon: {Longitude}", 
                        innmeldingModel.InnmeldingId, lokasjon.Latitude, lokasjon.Longitude);
                }

                await _innmeldingService.UpdateInnmeldingDetails(innmeldingModel, lokasjon, bilde);
                _logger.LogInformation("Innmelding {InnmeldingId} oppdatert", innmeldingModel.InnmeldingId);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding {InnmeldingId}", innmeldingModel.InnmeldingId);
                return RedirectToAction("Detaljer", new { id = innmeldingModel.InnmeldingId });
            }
        }

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