using System.Data;
using Dapper;
using MySqlConnector;
using KartverketGruppe5.Models;
using Microsoft.Extensions.Configuration;

namespace KartverketGruppe5.Services
{
    public class KommuneService
    {
        private readonly string _connectionString;
        private readonly ILogger<KommuneService> _logger;

        public KommuneService(IConfiguration configuration, ILogger<KommuneService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

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
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT kommuneId, fylkeId, navn, kommuneNummer 
                    FROM Kommune 
                    ORDER BY navn";
                    
                var result = await connection.QueryAsync<Kommune>(sql);
                return result.ToList();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching kommuner with term {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
} 