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

        public InnmeldingService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
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
    }
} 