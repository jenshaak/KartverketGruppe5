using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Controllers
{
    [Authorize]
    public class MineSakerController : Controller
    {
        private readonly InnmeldingService _innmeldingService;
        private readonly LokasjonService _lokasjonService;
        private readonly SaksbehandlerService _saksbehandlerService;
        private readonly ILogger<MineSakerController> _logger;

        public MineSakerController(InnmeldingService innmeldingService, LokasjonService lokasjonService, SaksbehandlerService saksbehandlerService, ILogger<MineSakerController> logger)
        {
            _innmeldingService = innmeldingService;
            _lokasjonService = lokasjonService;
            _saksbehandlerService = saksbehandlerService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var saksbehandlerId = int.Parse(User.FindFirst("SaksbehandlerId")?.Value ?? "0");
            var innmeldinger = await _innmeldingService.GetInnmeldinger(includeKommuneNavn: true, saksbehandlerId: saksbehandlerId);
            return View(innmeldinger);
        }

        public async Task<IActionResult> Behandle(int id)
        {
            var innmelding = await _innmeldingService.GetInnmeldingById(id);
            var lokasjon = _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
            var saksbehandlere = await _saksbehandlerService.GetAllSaksbehandlere();
            ViewBag.Lokasjon = lokasjon;
            ViewBag.Saksbehandlere = saksbehandlere;
            return View(innmelding);
        }
    }
}
