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
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<LokasjonViewModel>> GetAllLokasjoner()
        {
            try
            {
                return await _repository.GetAllLokasjoner();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil ved henting av alle lokasjoner");
                throw;
            }
        }

        public async Task<int> AddLokasjon(string geoJson, double latitude, double longitude, string geometriType)
        {
            try
            {
                return await _repository.AddLokasjon(geoJson, latitude, longitude, geometriType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil ved opprettelse av lokasjon: ({Latitude}, {Longitude})", 
                    latitude, longitude);
                throw;
            }
        }

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
                _logger.LogError(ex, "Uventet feil ved henting av lokasjon {LokasjonId}", id);
                throw;
            }
        }

        public async Task<int> GetKommuneIdFromCoordinates(double latitude, double longitude)
        {
            try
            {
                return await _repository.GetKommuneIdFromCoordinates(latitude, longitude);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil ved henting av kommuneId fra koordinater ({Latitude}, {Longitude})", 
                    latitude, longitude);
                throw;
            }
        }

        public async Task UpdateLokasjon(LokasjonViewModel lokasjon, DateTime oppdatertDato)
        {
            try
            {
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
                _logger.LogError(ex, "Uventet feil ved oppdatering av lokasjon {LokasjonId}", 
                    lokasjon.LokasjonId);
                throw;
            }
        }
    }
} 