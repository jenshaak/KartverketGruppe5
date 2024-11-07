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

        public IActionResult CorrectionOverview()
        {
            var positions = _lokasjonService.GetAllLokasjoner()
                .Select(l => new PositionModel
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Description = _innmeldingService.GetInnmeldingForLokasjon(l.LokasjonId)?.Beskrivelse,
                    GeoJson = l.GeoJson,
                    GeometriType = l.GeometriType
                })
                .ToList();

            return View(positions);
        }

        [HttpPost]
        public IActionResult CorrectionOverview(PositionModel model)
        {
            if (ModelState.IsValid)
            {
                int lokasjonId = _lokasjonService.AddLokasjon(
                    model.GeoJson,
                    model.Latitude,
                    model.Longitude,
                    model.GeometriType
                );

                _innmeldingService.AddInnmelding(
                    brukerId: 1,
                    kommuneId: 1,
                    lokasjonId: lokasjonId,
                    beskrivelse: model.Description
                );

                return RedirectToAction("CorrectionOverview", "MapChange");
            }
            return View(model);
        }
    }
} 