using System.Data;
using Dapper;
using MySqlConnector;
using KartverketGruppe5.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KartverketGruppe5.Services
{
    public class LokasjonService
    {
        private readonly string _connectionString;
        private readonly ILogger<LokasjonService> _logger;

        public LokasjonService(IConfiguration configuration, ILogger<LokasjonService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public List<LokasjonModel> GetAllLokasjoner()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT 
                        lokasjonId,
                        geoJson,
                        latitude,
                        longitude,
                        geometriType
                    FROM Lokasjon";

                return connection.Query<LokasjonModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all lokasjoner: {ex.Message}");
                return new List<LokasjonModel>();
            }
        }

        public int AddLokasjon(string geoJson, double latitude, double longitude, string geometriType)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO Lokasjon (geoJson, latitude, longitude, geometriType) 
                    VALUES (@GeoJson, @Latitude, @Longitude, @GeometriType);
                    SELECT LAST_INSERT_ID();";

                return connection.ExecuteScalar<int>(sql, new 
                { 
                    GeoJson = geoJson,
                    Latitude = latitude,
                    Longitude = longitude,
                    GeometriType = geometriType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding lokasjon: {ex.Message}");
                throw;
            }
        }

        public Lokasjon GetLokasjonById(int id)
        {
            try 
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = "SELECT * FROM Lokasjon WHERE lokasjonId = @Id";
                return connection.QuerySingleOrDefault<Lokasjon>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting lokasjon by id {id}: {ex.Message}");
                return null;
            }
        }
    }
} 