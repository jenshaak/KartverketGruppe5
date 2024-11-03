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
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Bruker> GetBrukerByEmail(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Bruker>(
                "SELECT * FROM bruker WHERE email = @Email", 
                new { Email = email });
        }

        public async Task<bool> CreateBruker(Bruker bruker)
        {
            bruker.Passord = HashPassword(bruker.Passord);
            
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO Bruker (Fornavn, Etternavn, Email, Passord) 
                    VALUES (@Fornavn, @Etternavn, @Email, @Passord)",
                    bruker);
                return true;
            }
            catch (MySqlException)
            {
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
                "UPDATE bruker SET rolle = @Rolle WHERE bruker_id = @BrukerId",
                new { Rolle = nyRolle, BrukerId = brukerId });
            return affected > 0;
        }

        public async Task<IEnumerable<Bruker>> GetAlleBrukere()
        {
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryAsync<Bruker>(
                "SELECT bruker_id, fornavn, etternavn, email, rolle, opprettet_dato FROM bruker ORDER BY opprettet_dato DESC");
        }
    }
} 