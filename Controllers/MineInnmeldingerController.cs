using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using KartverketGruppe5.Models;

namespace KartverketGruppe5.Controllers
{
    public class MineInnmeldingerController : Controller
    {
        private readonly InnmeldingService _innmeldingService;
        private readonly KommuneService _kommuneService;

        public MineInnmeldingerController(InnmeldingService innmeldingService, KommuneService kommuneService)
        {
            _innmeldingService = innmeldingService;
            _kommuneService = kommuneService;
        }

        public IActionResult Index()
        {
            // Sjekk om bruker er logget inn
            var brukerId = HttpContext.Session.GetInt32("BrukerId");
            if (brukerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var innmeldinger = _innmeldingService.GetMineInnmeldinger(brukerId.Value)
                .Select(i => new InnmeldingModel
                {
                    InnmeldingId = i.InnmeldingId,
                    KommuneNavn = _kommuneService.GetKommuneById(i.KommuneId)?.Navn ?? "Ukjent",
                    Beskrivelse = i.Beskrivelse,
                    Status = i.Status,
                    OpprettetDato = i.OpprettetDato,
                    Kommentar = i.Kommentar
                })
                .ToList();

            return View(innmeldinger);
        }
    }
} 