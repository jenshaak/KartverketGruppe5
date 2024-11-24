using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dapper;
using MySqlConnector;
using System.Data;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Repositories.Interfaces;

namespace KartverketGruppe5.Services
{
    public class FylkeService : IFylkeService
    {
        private readonly IFylkeRepository _repository;
        private readonly ILogger<FylkeService> _logger;

        public FylkeService(IFylkeRepository repository, ILogger<FylkeService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Henter alle fylker, med caching for bedre ytelse
        /// </summary>
        /// <returns>Liste over alle fylker</returns>
        public async Task<List<Fylke>> GetAllFylker()
        {
            try
            {
                var fylkeListe = await _repository.GetAllFylker();
                _logger.LogInformation("Hentet {AntallFylker} fylker", fylkeListe.Count);
                return fylkeListe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet feil ved henting av fylker");
                throw;
            }
        }
    }
}