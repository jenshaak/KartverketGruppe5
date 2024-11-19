using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace KartverketGruppe5.Controllers
{
    [Authorize]
    public class MineInnmeldingerController : Controller
    {
        private readonly InnmeldingService _innmeldingService;
        private readonly LokasjonService _lokasjonService;
        private readonly KommuneService _kommuneService;    
        private readonly ILogger<MineInnmeldingerController> _logger;
        public MineInnmeldingerController(InnmeldingService innmeldingService, LokasjonService lokasjonService, KommuneService kommuneService, ILogger<MineInnmeldingerController> logger)
        {
            _innmeldingService = innmeldingService;
            _lokasjonService = lokasjonService;
            _kommuneService = kommuneService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var brukerId = HttpContext.Session.GetInt32("BrukerId");
            if (brukerId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var innmeldinger = await _innmeldingService.GetInnmeldinger(innmelderBrukerId: brukerId.Value);
            return View(innmeldinger);
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
                StatusClass = GetStatusClass(innmelding.Status),
                BildeSti = innmelding.BildeSti ?? null
            };

            ViewBag.Lokasjon = lokasjon;
            return View(innmeldingModel);
        }

        [HttpPost]
        public async Task<IActionResult> EndreInnmelding(InnmeldingModel innmeldingModel, IFormFile bilde)
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

                LokasjonModel lokasjon = null;
                if (Request.Form["geoJsonInput"].Count > 0 && 
                    Request.Form["geometriType"].Count > 0 && 
                    Request.Form["latitude"].Count > 0 && 
                    Request.Form["longitude"].Count > 0 &&
                    !string.IsNullOrWhiteSpace(Request.Form["latitude"]) &&
                    !string.IsNullOrWhiteSpace(Request.Form["longitude"]))
                {
                    lokasjon = new LokasjonModel
                    {
                        GeoJson = Request.Form["geoJsonInput"],
                        GeometriType = Request.Form["geometriType"],
                        Latitude = double.Parse(Request.Form["latitude"]),
                        Longitude = double.Parse(Request.Form["longitude"])
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