

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
        private readonly ILogger<MineSakerController> _logger;

        public MineSakerController(InnmeldingService innmeldingService, ILogger<MineSakerController> logger)
        {
            _innmeldingService = innmeldingService;
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
            return View(innmelding);
        }
    }
}
