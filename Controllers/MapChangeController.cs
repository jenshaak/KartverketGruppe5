using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    public class MapChangeController : Controller
    {
        private readonly LokasjonService _lokasjonService;
        private readonly InnmeldingService _innmeldingService;

        public MapChangeController(LokasjonService lokasjonService, InnmeldingService innmeldingService)
        {
            _lokasjonService = lokasjonService;
            _innmeldingService = innmeldingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CorrectMap(PositionModel model)
        {
            if (ModelState.IsValid)
            {
                // Først lagre lokasjonen
                int lokasjonId = _lokasjonService.AddLokasjon(
                    model.GeoJson,
                    model.Latitude,
                    model.Longitude,
                    model.GeometriType
                );

                // Så lagre innmeldingen (du må implementere denne metoden)
                _innmeldingService.AddInnmelding(
                    brukerId: 1, // Dette bør komme fra innlogget bruker
                    kommuneId: 1, // Dette bør beregnes basert på koordinater
                    lokasjonId: lokasjonId,
                    beskrivelse: model.Description
                );

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
} 