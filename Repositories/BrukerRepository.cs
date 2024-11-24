using KartverketGruppe5.Models;
using KartverketGruppe5.Repositories.Interfaces;
using MySqlConnector;
using Dapper;

namespace KartverketGruppe5.Repositories
{
    public class BrukerRepository : IBrukerRepository
    {
        private readonly string _connectionString;

        public BrukerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Bruker?> GetByEmail(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Bruker>(
                "SELECT * FROM Bruker WHERE email = @Email", 
                new { Email = email });
        }

        public async Task<bool> Create(Bruker bruker)
        {
            using var connection = new MySqlConnection(_connectionString);
            var affected = await connection.ExecuteAsync(@"
                INSERT INTO Bruker (fornavn, etternavn, email, passord) 
                VALUES (@Fornavn, @Etternavn, @Email, @Passord)",
                bruker);
            return affected > 0;
        }

        public async Task<bool> Delete(int brukerId)
        {
            using var connection = new MySqlConnection(_connectionString);
            var affected = await connection.ExecuteAsync(
                "DELETE FROM Bruker WHERE brukerId = @BrukerId", 
                new { BrukerId = brukerId });
            return affected > 0;
        }

        public async Task<bool> Update(Bruker bruker)
        {
            using var connection = new MySqlConnection(_connectionString);
            var affected = await connection.ExecuteAsync(@"
                UPDATE Bruker
                SET fornavn = @Fornavn, 
                    etternavn = @Etternavn, 
                    email = @Email
                WHERE brukerId = @BrukerId",
                bruker);
            return affected > 0;
        }
    }
} 