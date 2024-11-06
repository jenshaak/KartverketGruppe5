using Dapper;
using MySqlConnector;
using System.Data;
using KartverketGruppe5.Models;

namespace KartverketGruppe5.Services
{
    public class LokasjonService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;

        public LokasjonService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public int AddLokasjon(string geoJson, double latitude, double longitude, string geometriType)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = @"INSERT INTO Lokasjon (geoJson, latitude, longitude, geometriType) 
                               VALUES (@GeoJson, @Latitude, @Longitude, @GeometriType);
                               SELECT LAST_INSERT_ID();";
                
                return dbConnection.ExecuteScalar<int>(query, new { 
                    GeoJson = geoJson,
                    Latitude = latitude,
                    Longitude = longitude,
                    GeometriType = geometriType
                });
            }
        }

        public Lokasjon GetLokasjonById(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = "SELECT * FROM Lokasjon WHERE lokasjonId = @Id";
                return dbConnection.QuerySingleOrDefault<Lokasjon>(query, new { Id = id });
            }
        }
    }
} 