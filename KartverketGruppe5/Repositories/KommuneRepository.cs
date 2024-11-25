using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;

namespace KartverketGruppe5.Repositories
{
    /// <summary>
    /// Repository for kommuner
    /// </summary>
    public class KommuneRepository : IKommuneRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<KommuneRepository> _logger;

        public KommuneRepository(IConfiguration configuration, ILogger<KommuneRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Henter kommune basert på ID
        /// </summary>
        public async Task<Kommune?> GetKommuneById(int kommuneId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT kommuneId, fylkeId, navn, kommuneNummer 
                    FROM Kommune 
                    WHERE kommuneId = @KommuneId";

                return await connection.QueryFirstOrDefaultAsync<Kommune>(sql, new { KommuneId = kommuneId });
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error ved henting av kommune {KommuneId}", kommuneId);
                throw;
            }
        }

        /// <summary>
        /// Henter alle kommuner
        /// </summary>
        public async Task<List<Kommune>> GetAllKommuner()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT kommuneId, fylkeId, navn, kommuneNummer 
                    FROM Kommune 
                    ORDER BY navn";
                    
                var result = await connection.QueryAsync<Kommune>(sql);
                return result.ToList();
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error ved henting av alle kommuner");
                throw;
            }
        }

        /// <summary>
        /// Søker etter kommuner med et gitt søkeord
        /// </summary>
        public async Task<List<Kommune>> SearchKommuner(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<Kommune>();
                }

                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT kommuneId, fylkeId, navn, kommuneNummer 
                    FROM Kommune 
                    WHERE navn LIKE @SearchTerm 
                    ORDER BY navn 
                    LIMIT 10";

                var result = await connection.QueryAsync<Kommune>(
                    sql, 
                    new { SearchTerm = $"%{searchTerm.Trim()}%" }
                );
                
                return result.ToList();
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error ved søk etter kommuner med søkeord {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
} 