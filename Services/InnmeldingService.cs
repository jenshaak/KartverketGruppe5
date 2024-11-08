using Dapper;
using MySqlConnector;
using System.Data;
using KartverketGruppe5.Models;
namespace KartverketGruppe5.Services
{
    public class InnmeldingService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;
        private readonly ILogger<InnmeldingService> _logger;

        public InnmeldingService(IConfiguration configuration, ILogger<InnmeldingService> logger)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public int AddInnmelding(int brukerId, int kommuneId, int lokasjonId, string beskrivelse)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = @"INSERT INTO Innmelding 
                               (brukerId, kommuneId, lokasjonId, beskrivelse, status) 
                               VALUES 
                               (@BrukerId, @KommuneId, @LokasjonId, @Beskrivelse, 'Ny');
                               SELECT LAST_INSERT_ID();";

                return dbConnection.ExecuteScalar<int>(query, new
                {
                    BrukerId = brukerId,
                    KommuneId = kommuneId,
                    LokasjonId = lokasjonId,
                    Beskrivelse = beskrivelse
                });
            }
        }

        public IEnumerable<Innmelding> GetAllInnmeldinger()
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = @"SELECT i.*, l.latitude, l.longitude, l.geoJson 
                               FROM Innmelding i 
                               JOIN Lokasjon l ON i.lokasjonId = l.lokasjonId";
                return dbConnection.Query<Innmelding>(query);
            }
        }

        public List<InnmeldingModel> GetMineInnmeldinger(int brukerId)
        {
            using var connection = new MySqlConnection(_connectionString);
            
            // Hent innmeldinger med kommunenavn i én spørring
            const string sql = @"
                SELECT 
                    i.innmeldingId,
                    i.brukerId,
                    i.kommuneId,
                    i.lokasjonId,
                    i.beskrivelse,
                    i.status,
                    i.opprettetDato,
                    k.navn as kommuneNavn
                FROM Innmelding i
                INNER JOIN Kommune k ON i.kommuneId = k.kommuneId
                WHERE i.brukerId = @BrukerId
                ORDER BY i.opprettetDato DESC";

            var innmeldinger = connection.Query<(int innmeldingId, int brukerId, int kommuneId, 
                int lokasjonId, string beskrivelse, string status, DateTime opprettetDato, string kommuneNavn)>
                (sql, new { BrukerId = brukerId });

            return innmeldinger.Select(i => new InnmeldingModel
            {
                InnmeldingId = i.innmeldingId,
                BrukerId = i.brukerId,
                KommuneId = i.kommuneId,
                LokasjonId = i.lokasjonId,
                Beskrivelse = i.beskrivelse,
                Status = i.status,
                OpprettetDato = i.opprettetDato,
                KommuneNavn = i.kommuneNavn,
                StatusClass = i.status switch
                {
                    "Ny" => "bg-blue-100 text-blue-800",
                    "Under behandling" => "bg-yellow-100 text-yellow-800",
                    "Fullført" => "bg-green-100 text-green-800",
                    "Avvist" => "bg-red-100 text-red-800",
                    _ => "bg-gray-100 text-gray-800"
                }
            }).ToList();
        }

        public Innmelding GetInnmeldingById(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = @"SELECT i.*, l.latitude, l.longitude, l.geoJson 
                               FROM Innmelding i 
                               JOIN Lokasjon l ON i.lokasjonId = l.lokasjonId 
                               WHERE i.innmeldingId = @Id";
                return dbConnection.QuerySingleOrDefault<Innmelding>(query, new { Id = id });
            }
        }

        public void UpdateStatus(int innmeldingId, string status)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = "UPDATE Innmelding SET status = @Status WHERE innmeldingId = @Id";
                dbConnection.Execute(query, new { Id = innmeldingId, Status = status });
            }
        }

        public void AddKommentar(int innmeldingId, string kommentar)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string query = "UPDATE Innmelding SET kommentar = @Kommentar WHERE innmeldingId = @Id";
                dbConnection.Execute(query, new { Id = innmeldingId, Kommentar = kommentar });
            }
        }

        public InnmeldingModel GetInnmeldingForLokasjon(int lokasjonId)
        {
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    const string sql = @"
                        SELECT 
                            innmeldingId,
                            brukerId,
                            kommuneId,
                            lokasjonId,
                            beskrivelse,
                            opprettetDato
                        FROM Innmelding 
                        WHERE lokasjonId = @LokasjonId
                        ORDER BY opprettetDato DESC
                        LIMIT 1";

                    return db.QueryFirstOrDefault<InnmeldingModel>(sql, new { LokasjonId = lokasjonId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting innmelding for lokasjon {lokasjonId}: {ex.Message}");
                return null;
            }
        }
    }
} 