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

        public Kommune GetKommuneById(int kommuneId)
        {
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    const string sql = @"
                        SELECT kommuneId, fylkeId, navn, kommuneNummer 
                        FROM Kommune 
                        WHERE kommuneId = @KommuneId";

                    return db.QueryFirstOrDefault<Kommune>(sql, new { KommuneId = kommuneId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting kommune by id {kommuneId}: {ex.Message}");
                return null;
            }
        }

        public List<Kommune> GetAllKommuner()
        {
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    const string sql = "SELECT kommuneId, fylkeId, navn, kommuneNummer FROM Kommune";
                    return db.Query<Kommune>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all kommuner: {ex.Message}");
                return new List<Kommune>();
            }
        }
    }
} 