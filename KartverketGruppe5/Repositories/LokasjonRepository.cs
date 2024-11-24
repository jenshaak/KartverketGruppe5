using KartverketGruppe5.Models.ViewModels;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;
using System.Text.Json;

namespace KartverketGruppe5.Repositories
{
    public class LokasjonRepository : ILokasjonRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<LokasjonRepository> _logger;
        private readonly HttpClient _httpClient;

        // SQL-spørringer som konstanter for bedre vedlikehold
        private const string SELECT_LOKASJON_BASE = @"
            SELECT 
                lokasjonId,
                geoJson,
                latitude,
                longitude,
                geometriType
            FROM Lokasjon";

        public LokasjonRepository(
            IConfiguration configuration, 
            ILogger<LokasjonRepository> logger,
            IHttpClientFactory httpClientFactory)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "KartverketGruppe5");
        }

        /// <summary>
        /// Henter alle lokasjoner
        /// </summary>
        public async Task<List<LokasjonViewModel>> GetAllLokasjoner()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var result = await connection.QueryAsync<LokasjonViewModel>(SELECT_LOKASJON_BASE);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av alle lokasjoner");
                throw;
            }
        }

        /// <summary>
        /// Legger til ny lokasjon
        /// </summary>
        /// <returns>ID for den nye lokasjonen</returns>
        public async Task<int> AddLokasjon(string geoJson, double latitude, double longitude, string geometriType)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO Lokasjon (geoJson, latitude, longitude, geometriType) 
                    VALUES (@GeoJson, @Latitude, @Longitude, @GeometriType);
                    SELECT LAST_INSERT_ID();";

                var parameters = new 
                { 
                    GeoJson = geoJson,
                    Latitude = latitude,
                    Longitude = longitude,
                    GeometriType = geometriType
                };

                return await connection.ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved innsetting av lokasjon ({Latitude}, {Longitude})", 
                    latitude, longitude);
                throw;
            }
        }

        /// <summary>
        /// Henter lokasjon basert på ID
        /// </summary>
        public async Task<LokasjonViewModel?> GetLokasjonById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var sql = $"{SELECT_LOKASJON_BASE} WHERE lokasjonId = @Id";
                
                return await connection.QueryFirstOrDefaultAsync<LokasjonViewModel>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error ved henting av lokasjon {LokasjonId}", id);
                throw;
            }
        }

        /// <summary>
        /// Henter kommune-ID basert på koordinater ved hjelp av OpenStreetMap
        /// </summary>
        public async Task<int> GetKommuneIdFromCoordinates(double latitude, double longitude)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}";
                var response = await _httpClient.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonDocument>(response);
                
                if (data?.RootElement.TryGetProperty("address", out var address) != true)
                {
                    throw new Exception("Kunne ikke finne adresseinformasjon for koordinatene");
                }

                string? kommuneNavn = null;
                if (address.TryGetProperty("municipality", out var kommuneElement))
                {
                    kommuneNavn = kommuneElement.GetString();
                }
                else if (address.TryGetProperty("city", out var byElement))
                {
                    kommuneNavn = byElement.GetString();
                }

                if (string.IsNullOrEmpty(kommuneNavn))
                {
                    throw new Exception("Kunne ikke finne kommune for disse koordinatene");
                }

                using var connection = new MySqlConnection(_connectionString);
                const string sql = "SELECT kommuneId FROM Kommune WHERE navn LIKE @Navn";
                var kommuneId = await connection.QuerySingleOrDefaultAsync<int>(sql, new { Navn = $"%{kommuneNavn}%" });

                if (kommuneId == 0)
                {
                    throw new KeyNotFoundException($"Kommune {kommuneNavn} ikke funnet i databasen");
                }

                return kommuneId;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Feil ved henting av kommune fra koordinater ({Latitude}, {Longitude})", 
                    latitude, longitude);
                throw;
            }
        }

        /// <summary>
        /// Oppdaterer eksisterende lokasjon
        /// </summary>
        public async Task UpdateLokasjon(LokasjonViewModel lokasjon, DateTime oppdatertDato)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    UPDATE Lokasjon 
                    SET 
                        geoJson = @GeoJson,
                        geometriType = @GeometriType,
                        latitude = @Latitude,
                        longitude = @Longitude,
                        oppdatertDato = @OppdatertDato
                    WHERE lokasjonId = @LokasjonId";

                var parameters = new
                { 
                    lokasjon.GeoJson, 
                    lokasjon.GeometriType, 
                    lokasjon.Latitude, 
                    lokasjon.Longitude, 
                    OppdatertDato = oppdatertDato, 
                    lokasjon.LokasjonId 
                };

                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"Lokasjon med ID {lokasjon.LokasjonId} finnes ikke");
                }
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Database error ved oppdatering av lokasjon {LokasjonId}", lokasjon.LokasjonId);
                throw;
            }
        }

        /// <summary>
        /// Henter kommunenavn fra OpenStreetMap basert på koordinater
        /// </summary>
        private async Task<string> GetKommuneNavnFromOpenStreetMap(double latitude, double longitude)
        {
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}";
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<JsonDocument>(response);
            
            if (data?.RootElement.TryGetProperty("address", out var address) != true)
            {
                throw new Exception("Kunne ikke finne adresseinformasjon for koordinatene");
            }

            // Prøv å finne kommunenavn i ulike felter
            string? kommuneNavn = null;
            if (address.TryGetProperty("municipality", out var kommuneElement))
            {
                kommuneNavn = kommuneElement.GetString();
            }
            else if (address.TryGetProperty("city", out var byElement))
            {
                kommuneNavn = byElement.GetString();
            }

            if (string.IsNullOrEmpty(kommuneNavn))
            {
                throw new Exception("Kunne ikke finne kommune for disse koordinatene");
            }

            return kommuneNavn;
        }
    }
} 