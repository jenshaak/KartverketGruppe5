using MySqlConnector;
using Dapper;
using KartverketGruppe5.Models;
using System.Security.Cryptography;
using System.Text;

namespace KartverketGruppe5.Services
{
    public class SaksbehandlerService
    {
        private readonly string _connectionString;
        private readonly ILogger<SaksbehandlerService> _logger;

        public SaksbehandlerService(IConfiguration configuration, ILogger<SaksbehandlerService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<Saksbehandler> GetSaksbehandlerById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = "SELECT * FROM Saksbehandler WHERE saksbehandlerId = @Id";
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting saksbehandler: {ex.Message}");
                return null;
            }
        }

        public async Task<Saksbehandler> GetSaksbehandlerByEmail(string email)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = "SELECT * FROM Saksbehandler WHERE email = @Email";
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Email = email });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting saksbehandler by email: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Saksbehandler>> GetAllSaksbehandlere()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = "SELECT * FROM Saksbehandler ORDER BY opprettetDato DESC";
                var result = await connection.QueryAsync<Saksbehandler>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all saksbehandlere: {ex.Message}");
                return new List<Saksbehandler>();
            }
        }

        public async Task<bool> CreateSaksbehandler(Saksbehandler saksbehandler)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                saksbehandler.Passord = HashPassword(saksbehandler.Passord);

                const string sql = @"
                    INSERT INTO Saksbehandler (fornavn, etternavn, email, passord, admin)
                    VALUES (@Fornavn, @Etternavn, @Email, @Passord, @Admin)";
                
                var rowsAffected = await connection.ExecuteAsync(sql, saksbehandler);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating saksbehandler: {ex.Message}");
                return false;
            }
        }

        public bool UpdateSaksbehandler(Saksbehandler saksbehandler)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    UPDATE Saksbehandler 
                    SET fornavn = @Fornavn,
                        etternavn = @Etternavn,
                        email = @Email,
                        admin = @Admin
                    WHERE saksbehandlerId = @SaksbehandlerId";
                
                var rowsAffected = connection.Execute(sql, saksbehandler);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating saksbehandler: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateSaksbehandlerRolle(int saksbehandlerId, string nyRolle)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = "UPDATE Saksbehandler SET admin = @NyRolle WHERE saksbehandlerId = @SaksbehandlerId";
                var rowsAffected = await connection.ExecuteAsync(sql, new { NyRolle = nyRolle, SaksbehandlerId = saksbehandlerId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating saksbehandler role: {ex.Message}");
                return false;
            }
        }

        public List<InnmeldingModel> GetAllInnmeldinger()
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
                ORDER BY i.opprettetDato DESC";

            var innmeldinger = connection.Query<(int innmeldingId, int brukerId, int kommuneId, 
                int lokasjonId, string beskrivelse, string status, DateTime opprettetDato, string kommuneNavn)>
                (sql);

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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
} 