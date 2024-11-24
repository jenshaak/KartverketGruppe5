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
            _repository = repository;
            _logger = logger;
        }

        public async Task<Kommune?> GetKommuneById(int kommuneId)
        {
            try
            {
                return await _repository.GetKommuneById(kommuneId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting kommune with id {KommuneId}", kommuneId);
                throw;
            }
        }

        public async Task<List<Kommune>> GetAllKommuner()
        {
            try
            {
                return await _repository.GetAllKommuner();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all kommuner");
                throw;
            }
        }

        public async Task<List<Kommune>> SearchKommuner(string searchTerm)
        {
            try
            {
                return await _repository.SearchKommuner(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching kommuner with term {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
} 