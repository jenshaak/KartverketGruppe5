using KartverketGruppe5.Models;
using MySqlConnector;
using Dapper;
using System.Security.Cryptography;
using System.Text;

namespace KartverketGruppe5.Services
{
    public class BrukerService
    {
        private readonly string _connectionString;

        public BrukerService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Bruker> GetBrukerByEmail(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Bruker>(
                "SELECT * FROM Bruker WHERE email = @email", 
                new { email = email });
        }

        public async Task<bool> CreateBruker(Bruker bruker)
        {
            bruker.Passord = HashPassword(bruker.Passord);
            
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO Bruker (fornavn, etternavn, email, passord) 
                    VALUES (@fornavn, @etternavn, @email, @passord)",
                    bruker);
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                return false;
            }
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

        public async Task<bool> OppdaterBrukerRolle(int brukerId, string nyRolle)
        {
            using var connection = new MySqlConnection(_connectionString);
            var affected = await connection.ExecuteAsync(
                "UPDATE Bruker SET rolle = @Rolle WHERE brukerId = @BrukerId",
                new { Rolle = nyRolle, BrukerId = brukerId });
            return affected > 0;
        }

        public async Task<IEnumerable<Bruker>> GetAlleBrukere()
        {
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryAsync<Bruker>(
                "SELECT brukerId, fornavn, etternavn, email, rolle, opprettetDato FROM Bruker ORDER BY opprettetDato DESC");
        }
    }
} 