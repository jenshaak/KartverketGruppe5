using KartverketGruppe5.Models;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Repositories.Interfaces;
using KartverketGruppe5.Services.Interfaces;

namespace KartverketGruppe5.Services
{
    public class BrukerService : IBrukerService
    {
        private readonly IBrukerRepository _repository;
        private readonly ILogger<BrukerService> _logger;
        private readonly IPasswordService _passwordService;

        public BrukerService(
            IBrukerRepository repository,
            ILogger<BrukerService> logger,
            IPasswordService passwordService)
        {
            _repository = repository;
            _logger = logger;
            _passwordService = passwordService;
        }

        public async Task<Bruker?> GetBrukerByEmail(string email)
        {
            try
            {
                return await _repository.GetByEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved henting av bruker med email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> CreateBruker(Bruker bruker)
        {
            try
            {
                // Sjekk om emailen allerede er i bruk
                if (await GetBrukerByEmail(bruker.Email) != null)
                {
                    _logger.LogWarning("Forsøk på å opprette bruker med eksisterende email: {Email}", bruker.Email);
                    return false;
                }

                bruker.Passord = _passwordService.HashPassword(bruker.Passord);
                return await _repository.Create(bruker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved opprettelse av bruker: {Email}", bruker.Email);
                return false;
            }
        }

        public async Task<bool> DeleteBruker(int brukerId)
        {   
            try
            {
                return await _repository.Delete(brukerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved sletting av bruker med ID: {BrukerId}", brukerId);
                return false;
            }
        }

        public async Task<bool> OppdaterBruker(Bruker bruker)
        {
            try
            {
                return await _repository.Update(bruker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av bruker");
                return false;
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return _passwordService.VerifyPassword(password, hashedPassword);
        }
    }
} 