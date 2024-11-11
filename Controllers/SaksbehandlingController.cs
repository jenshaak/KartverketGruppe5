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
        private readonly ILogger<SaksbehandlingController> _logger;

        public SaksbehandlingController(SaksbehandlerService saksbehandlerService, InnmeldingService innmeldingService, LokasjonService lokasjonService, KommuneService kommuneService, ILogger<SaksbehandlingController> logger)
        {
            _saksbehandlerService = saksbehandlerService;
            _innmeldingService = innmeldingService;
            _lokasjonService = lokasjonService;
            _kommuneService = kommuneService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Saksbehandler") && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _logger.LogInformation("Henter alle innmeldinger");
            var innmeldinger = _saksbehandlerService.GetAllInnmeldinger();
            _logger.LogInformation("Fant {AntallInnmeldinger} innmeldinger", innmeldinger.Count);
            return View(innmeldinger);
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

        [HttpPost]
        public async Task<IActionResult> EndreRolle(int saksbehandlerId, string nyRolle)
        {
            var success = await _saksbehandlerService.UpdateSaksbehandlerRolle(saksbehandlerId, nyRolle);
            if (!success)
            {
                _logger.LogError("Kunne ikke oppdatere rolle til {NyRolle} for bruker med ID: {SaksbehandlerId}", nyRolle, saksbehandlerId);
                return BadRequest("Kunne ikke oppdatere brukerrolle");
            }
            return RedirectToAction("Index", "Admin");
        }

        // public async Task<IActionResult> Detaljer(int id)
        // {
        //     try 
        //     {
        //         var innmelding = await _saksbehandlerService.GetInnmeldingById(id);
        //         if (innmelding == null)
        //         {
        //             _logger.LogWarning("Fant ikke innmelding med ID: {Id}", id);
        //             return NotFound();
        //         }

        //         _logger.LogInformation("Hentet detaljer for innmelding {Id}", id);
        //         return View(innmelding);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Feil ved henting av innmelding {Id}", id);
        //         return RedirectToAction("Index");
        //     }
        // }

        public IActionResult Detaljer(int id)
        {
            var innmelding = _innmeldingService.GetInnmeldingById(id);
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
                StatusClass = GetStatusClass(innmelding.Status)
            };

            ViewBag.Lokasjon = lokasjon;
            Console.WriteLine($"Lokasjon: {lokasjon?.Latitude}, {lokasjon?.Longitude}, GeoJson: {lokasjon?.GeoJson}");
            return View(innmeldingModel);
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
