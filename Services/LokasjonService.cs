using System.Data;
using Dapper;
using MySqlConnector;
using KartverketGruppe5.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;

namespace KartverketGruppe5.Services
{
    public class LokasjonService
    {
        private readonly string _connectionString;
        private readonly ILogger<LokasjonService> _logger;

        public LokasjonService(IConfiguration configuration, ILogger<LokasjonService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public List<LokasjonViewModel> GetAllLokasjoner()
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

                return connection.Query<LokasjonViewModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all lokasjoner: {ex.Message}");
                return new List<LokasjonViewModel>();
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

        public LokasjonViewModel GetLokasjonById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT lokasjonId, latitude, longitude, geoJson
                    FROM Lokasjon 
                    WHERE lokasjonId = @Id";

                return connection.QueryFirstOrDefault<LokasjonViewModel>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting lokasjon: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetKommuneIdFromCoordinates(double latitude, double longitude)
        {
            try
            {
                using var client = new HttpClient();
                // Legg til User-Agent header som er påkrevd av Nominatim
                client.DefaultRequestHeaders.Add("User-Agent", "KartverketGruppe5");
                
                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}";
                var response = await client.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonDocument>(response);
                
                // Hent kommune fra address objektet
                var address = data.RootElement.GetProperty("address");
                string municipality;
                
                // Nominatim kan returnere kommune i forskjellige felter
                if (address.TryGetProperty("municipality", out var municipalityElement))
                {
                    municipality = municipalityElement.GetString();
                }
                else if (address.TryGetProperty("city", out var cityElement))
                {
                    municipality = cityElement.GetString();
                }
                else
                {
                    throw new Exception("Kunne ikke finne kommune for disse koordinatene");
                }

                // Finn kommune ID fra databasen basert på navn
                using var connection = new MySqlConnection(_connectionString);
                const string sql = "SELECT kommuneId FROM Kommune WHERE navn LIKE @Navn";
                var kommuneId = await connection.QuerySingleOrDefaultAsync<int>(
                    sql, 
                    new { Navn = $"%{municipality}%" }
                );

                if (kommuneId == 0)
                {
                    _logger.LogError($"Kommune {municipality} ikke funnet i databasen");
                    throw new Exception("Kommune ikke funnet i databasen");
                }

                return kommuneId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting kommune from coordinates: {ex.Message}");
                throw;
            }
        }
    }
} 