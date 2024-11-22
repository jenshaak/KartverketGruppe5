using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using KartverketGruppe5.Models.RequestModels;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "Bruker")]
    public class MineInnmeldingerController : Controller
    {
        private readonly InnmeldingService _innmeldingService;
        private readonly LokasjonService _lokasjonService;
        private readonly KommuneService _kommuneService;        
        private readonly FylkeService _fylkeService;
        private readonly ILogger<MineInnmeldingerController> _logger;
        public MineInnmeldingerController(InnmeldingService innmeldingService, LokasjonService lokasjonService, KommuneService kommuneService, FylkeService fylkeService, ILogger<MineInnmeldingerController> logger)
        {
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
            SetupViewData(sortOrder, statusFilter, fylkeFilter);

            var brukerId = HttpContext.Session.GetInt32("BrukerId");
            if (brukerId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try 
            {
                ViewBag.Fylker = await _fylkeService.GetAllFylker();
                
                var request = new InnmeldingRequest
                {
                    InnmelderBrukerId = brukerId.Value,
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
            ViewData["Statuses"] = new List<string> 
            { 
                "Ny",
                "Under behandling",
                "Godkjent",
                "Avvist"
            };
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

            _logger.LogInformation($"Bildestien: {innmelding.BildeSti}");

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
                BildeSti = innmelding.BildeSti ?? null
            };

            ViewBag.Lokasjon = lokasjon;

            _logger.LogInformation($"Kommentar: {innmeldingViewModel.Kommentar}");
            return View(innmeldingViewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> EndreInnmelding(InnmeldingViewModel innmeldingModel, IFormFile bilde)
        {
            try 
            {
                _logger.LogInformation($"InnmeldingId: {innmeldingModel.InnmeldingId}");
                _logger.LogInformation($"Beskrivelse: {innmeldingModel.Beskrivelse}");
                
                if (bilde != null)
                {
                    _logger.LogInformation($"Bilde info:");
                    _logger.LogInformation($" - Navn: {bilde.FileName}");
                    _logger.LogInformation($" - Størrelse: {bilde.Length} bytes");
                    _logger.LogInformation($" - Content Type: {bilde.ContentType}");
                }
                else
                {
                    _logger.LogInformation("Ingen ny fil lastet opp");
                }

                LokasjonViewModel? lokasjon = null;
                if (Request.Form.TryGetValue("geoJsonInput", out var geoJson) && 
                    Request.Form.TryGetValue("geometriType", out var geometriType) && 
                    Request.Form.TryGetValue("latitude", out var latitude) && 
                    Request.Form.TryGetValue("longitude", out var longitude) &&
                    !string.IsNullOrWhiteSpace(latitude.ToString()) &&
                    !string.IsNullOrWhiteSpace(longitude.ToString()))
                {
                    lokasjon = new LokasjonViewModel
                    {
                        GeoJson = geoJson.ToString(),
                        GeometriType = geometriType.ToString(),
                        Latitude = double.Parse(latitude.ToString()),
                        Longitude = double.Parse(longitude.ToString())
                    };
                    
                    _logger.LogInformation($"Oppdaterer lokasjon:");
                    _logger.LogInformation($" - GeoJson: {lokasjon.GeoJson}");
                    _logger.LogInformation($" - GeometriType: {lokasjon.GeometriType}");
                    _logger.LogInformation($" - Latitude: {lokasjon.Latitude}");
                    _logger.LogInformation($" - Longitude: {lokasjon.Longitude}");
                }
                else
                {
                    _logger.LogInformation("Ingen endring i lokasjon");
                }

                await _innmeldingService.UpdateInnmeldingDetails(innmeldingModel, lokasjon, bilde);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av innmelding");
                return RedirectToAction("Detaljer", new { id = innmeldingModel.InnmeldingId });
            }
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