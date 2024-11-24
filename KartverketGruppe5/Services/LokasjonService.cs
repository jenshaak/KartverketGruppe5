using System.Data;
using Dapper;
using MySqlConnector;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Repositories.Interfaces;
namespace KartverketGruppe5.Services
{
    public class LokasjonService : ILokasjonService
    {
        private readonly ILokasjonRepository _repository;
        private readonly ILogger<LokasjonService> _logger;

        public LokasjonService(
            ILokasjonRepository repository,
            ILogger<LokasjonService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Henter alle lokasjoner
        /// </summary>
        public async Task<List<LokasjonViewModel>> GetAllLokasjoner()
        {
            try
            {
                return await _repository.GetAllLokasjoner();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av alle lokasjoner");
                throw;
            }
        }

        /// <summary>
        /// Legger til en ny lokasjon
        /// </summary>
        /// <returns>ID for den nye lokasjonen</returns>
        public async Task<int> AddLokasjon(string geoJson, double latitude, double longitude, string geometriType)
        {
            try
            {
                ValidateCoordinates(latitude, longitude);
                ValidateGeometriType(geometriType);

                var lokasjonId = await _repository.AddLokasjon(geoJson, latitude, longitude, geometriType);
                _logger.LogInformation("Opprettet ny lokasjon med ID {LokasjonId} på koordinater ({Latitude}, {Longitude})", 
                    lokasjonId, latitude, longitude);
                
                return lokasjonId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved opprettelse av lokasjon på koordinater ({Latitude}, {Longitude})", 
                    latitude, longitude);
                throw;
            }
        }

        /// <summary>
        /// Henter en spesifikk lokasjon basert på ID
        /// </summary>
        public async Task<LokasjonViewModel?> GetLokasjonById(int id)
        {
            try
            {
                var lokasjon = await _repository.GetLokasjonById(id);
                if (lokasjon == null)
                {
                    _logger.LogWarning("Ingen lokasjon funnet med ID {LokasjonId}", id);
                }
                return lokasjon;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av lokasjon {LokasjonId}", id);
                throw;
            }
        }

        /// <summary>
        /// Henter kommune-ID basert på koordinater
        /// </summary>
        public async Task<int> GetKommuneIdFromCoordinates(double latitude, double longitude)
        {
            try
            {
                ValidateCoordinates(latitude, longitude);
                
                var kommuneId = await _repository.GetKommuneIdFromCoordinates(latitude, longitude);
                _logger.LogInformation("Fant kommune {KommuneId} for koordinater ({Latitude}, {Longitude})", 
                    kommuneId, latitude, longitude);
                
                return kommuneId;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av kommuneId fra koordinater ({Latitude}, {Longitude})", 
                    latitude, longitude);
                throw;
            }
        }

        /// <summary>
        /// Oppdaterer en eksisterende lokasjon
        /// </summary>
        public async Task UpdateLokasjon(LokasjonViewModel lokasjon, DateTime oppdatertDato)
        {
            try
            {
                ValidateCoordinates(lokasjon.Latitude, lokasjon.Longitude);
                ValidateGeometriType(lokasjon.GeometriType);

                await _repository.UpdateLokasjon(lokasjon, oppdatertDato);
                _logger.LogInformation("Lokasjon {LokasjonId} ble oppdatert", lokasjon.LokasjonId);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av lokasjon {LokasjonId}", lokasjon.LokasjonId);
                throw;
            }
        }

        /// <summary>
        /// Validerer at koordinatene er innenfor gyldige verdier
        /// </summary>
        private void ValidateCoordinates(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude må være mellom -90 og 90");
            }
            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude må være mellom -180 og 180");
            }
        }

        /// <summary>
        /// Validerer at geometritypen er gyldig
        /// </summary>
        private void ValidateGeometriType(string geometriType)
        {
            var validTypes = new[] { "point", "polyline", "polygon", "rectangle", "marker" };
            if (!validTypes.Contains(geometriType.ToLower()))
            {
                throw new ArgumentException($"Ugyldig geometritype. Må være en av: {string.Join(", ", validTypes)}, denne var: {geometriType}", 
                    nameof(geometriType));
            }
        }
    }
} 