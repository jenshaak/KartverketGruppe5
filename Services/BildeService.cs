using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KartverketGruppe5.Services
{
    public class BildeService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BildeService> _logger;
        private const string BILDEMAPPE = "innmeldingsbilder";

        public BildeService(IWebHostEnvironment webHostEnvironment, ILogger<BildeService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string?> LagreBilde(IFormFile? bilde, int innmeldingId)
        {
            if (bilde == null || bilde.Length == 0)
            {
                _logger.LogInformation("Ingen bilde ble lastet opp for innmelding {InnmeldingId}", innmeldingId);
                return null;
            }

            try
            {
                var bildemappe = OpprettBildemappe();
                var filnavn = GenererFilnavn(innmeldingId, bilde.FileName);
                var filsti = Path.Combine(bildemappe, filnavn);

                await LagreBildeFil(bilde, filsti);
                
                return Path.Combine(BILDEMAPPE, filnavn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved lagring av bilde for innmelding {InnmeldingId}", innmeldingId);
                throw;
            }
        }

        private string OpprettBildemappe()
        {
            var bildemappe = Path.Combine(_webHostEnvironment.WebRootPath, BILDEMAPPE);
            
            if (!Directory.Exists(bildemappe))
            {
                _logger.LogInformation("Oppretter bildemappe: {Bildemappe}", bildemappe);
                Directory.CreateDirectory(bildemappe);
            }

            return bildemappe;
        }

        private string GenererFilnavn(int innmeldingId, string originalFilnavn)
        {
            return $"{innmeldingId}_{DateTime.Now.Ticks}{Path.GetExtension(originalFilnavn)}";
        }

        private async Task LagreBildeFil(IFormFile bilde, string filsti)
        {
            _logger.LogInformation("Lagrer bilde til: {Filsti}", filsti);
            
            using var stream = new FileStream(filsti, FileMode.Create);
            await bilde.CopyToAsync(stream);
        }
    }
} 