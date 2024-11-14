using MySqlConnector;
using Dapper;
using KartverketGruppe5.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using KartverketGruppe5.Data;

namespace KartverketGruppe5.Services
{
    public class SaksbehandlerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SaksbehandlerService> _logger;
        private readonly string _connectionString;

        public SaksbehandlerService(ApplicationDbContext context, ILogger<SaksbehandlerService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Saksbehandler> GetSaksbehandlerById(int id)
        {
            try
            {
                return await _context.Saksbehandlere.FindAsync(id);
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
                return await _context.Saksbehandlere
                    .OrderByDescending(s => s.OpprettetDato)
                    .ToListAsync();
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
                saksbehandler.Passord = HashPassword(saksbehandler.Passord);
                _context.Saksbehandlere.Add(saksbehandler);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating saksbehandler: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateSaksbehandler(Saksbehandler saksbehandler)
        {
            try
            {
                var existing = await _context.Saksbehandlere.FindAsync(saksbehandler.SaksbehandlerId);
                if (existing == null) return false;

                if (!string.IsNullOrEmpty(saksbehandler.Passord))
                {
                    saksbehandler.Passord = HashPassword(saksbehandler.Passord);
                }
                else
                {
                    saksbehandler.Passord = existing.Passord;
                }

                _context.Entry(existing).CurrentValues.SetValues(saksbehandler);
                await _context.SaveChangesAsync();
                return true;
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