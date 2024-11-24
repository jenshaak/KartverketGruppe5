using System.Data;
using Dapper;
using MySqlConnector;
using KartverketGruppe5.Models;
using Microsoft.Extensions.Configuration;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Repositories.Interfaces;

namespace KartverketGruppe5.Services
{
    public class KommuneService : IKommuneService
    {
        private readonly IKommuneRepository _repository;
        private readonly ILogger<KommuneService> _logger;

        public KommuneService(IKommuneRepository repository, ILogger<KommuneService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Henter en spesifikk kommune basert på ID
        /// </summary>
        /// <param name="kommuneId">ID for kommunen som skal hentes</param>
        /// <returns>Kommune hvis funnet, null hvis ikke</returns>
        public async Task<Kommune?> GetKommuneById(int kommuneId)
        {
            try
            {
                return await _repository.GetKommuneById(kommuneId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av kommune med id {KommuneId}", kommuneId);
                throw;
            }
        }

        /// <summary>
        /// Henter alle kommuner
        /// </summary>
        /// <returns>Liste over alle kommuner</returns>
        public async Task<List<Kommune>> GetAllKommuner()
        {
            try
            {
                return await _repository.GetAllKommuner();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av alle kommuner");
                throw;
            }
        }

        /// <summary>
        /// Søker etter kommuner basert på søkeord
        /// </summary>
        /// <param name="searchTerm">Søkeord for kommunenavn</param>
        /// <returns>Liste over matchende kommuner</returns>
        public async Task<List<Kommune>> SearchKommuner(string searchTerm)
        {
            try
            {
                return await _repository.SearchKommuner(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved søk etter kommuner med søkeord: {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
} 