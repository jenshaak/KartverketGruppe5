using MySqlConnector;
using Dapper;
using KartverketGruppe5.Models;
using System.Security.Cryptography;
using System.Text;
using KartverketGruppe5.Models.ViewModels;
namespace KartverketGruppe5.Services
{
    public class SaksbehandlerService
    {
        private readonly ILogger<SaksbehandlerService> _logger;
        private readonly string _connectionString;

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
                const string sql = @"
                    SELECT * FROM Saksbehandler 
                    WHERE saksbehandlerId = @Id";
                
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting saksbehandler: {ex.Message}");
                return null;
            }
        }

        public async Task<Saksbehandler?> GetSaksbehandlerByEmail(string email)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    SELECT * FROM Saksbehandler 
                    WHERE email = @Email";
                
                return await connection.QueryFirstOrDefaultAsync<Saksbehandler>(sql, new { Email = email });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting saksbehandler by email: {ex.Message}");
                return null;
            }
        }

        public async Task<PagedResult<Saksbehandler>> GetAllSaksbehandlere(
            string sortOrder = "date_desc", 
            int page = 1, 
            int pageSize = 10)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                
                // Base query for counting total items
                const string countSql = "SELECT COUNT(*) FROM Saksbehandler";
                var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

                // Build the main query with sorting
                var orderByClause = sortOrder switch
                {
                    "admin_desc" => "admin DESC, opprettetDato DESC",
                    "admin_asc" => "admin ASC, opprettetDato DESC",
                    "date_asc" => "opprettetDato ASC",
                    _ => "opprettetDato DESC"
                };

                var sql = $@"
                    SELECT * FROM Saksbehandler
                    ORDER BY {orderByClause}
                    LIMIT @Skip, @Take";

                var items = await connection.QueryAsync<Saksbehandler>(sql, new
                {
                    Skip = (page - 1) * pageSize,
                    Take = pageSize
                });

                return new PagedResult<Saksbehandler>
                {
                    Items = items.ToList(),
                    TotalItems = totalItems,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all saksbehandlere: {ex.Message}");
                return new PagedResult<Saksbehandler>
                {
                    Items = new List<Saksbehandler>(),
                    TotalItems = 0,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
        }

        public async Task<bool> CreateSaksbehandler(Saksbehandler saksbehandler)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO Saksbehandler (
                        fornavn, etternavn, email, passord, admin, opprettetDato
                    ) VALUES (
                        @Fornavn, @Etternavn, @Email, @Passord, @Admin, @OpprettetDato
                    )";

                saksbehandler.Passord = HashPassword(saksbehandler.Passord);
                saksbehandler.OpprettetDato = DateTime.Now;

                var rowsAffected = await connection.ExecuteAsync(sql, saksbehandler);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating saksbehandler: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateSaksbehandler(SaksbehandlerRegistrerViewModel saksbehandler)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                
                // Hent eksisterende passord hvis ikke nytt er angitt
                if (string.IsNullOrEmpty(saksbehandler.Passord))
                {
                    var existing = await GetSaksbehandlerById(saksbehandler.SaksbehandlerId);
                    if (existing == null) return false;
                    saksbehandler.Passord = existing.Passord;
                }
                else
                {
                    saksbehandler.Passord = HashPassword(saksbehandler.Passord);
                }

                const string sql = @"
                    UPDATE Saksbehandler 
                    SET fornavn = @Fornavn,
                        etternavn = @Etternavn,
                        email = @Email,
                        passord = @Passord,
                        admin = @Admin
                    WHERE saksbehandlerId = @SaksbehandlerId";

                var rowsAffected = await connection.ExecuteAsync(sql, saksbehandler);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating saksbehandler: {ex.Message}");
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

    }
} 