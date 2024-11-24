using KartverketGruppe5.Models;
using Microsoft.Extensions.Logging;
using KartverketGruppe5.Repositories.Interfaces;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.Models.RequestModels;
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
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        /// <summary>
        /// Henter bruker basert på email
        /// </summary>
        /// <returns>Bruker hvis funnet, null hvis ikke</returns>
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

        /// <summary>
        /// Oppretter ny bruker
        /// </summary>
        /// <returns>true hvis vellykket, false hvis bruker allerede eksisterer eller ved feil</returns>
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

        /// <summary>
        /// Oppdaterer eksisterende bruker
        /// </summary>
        /// <returns>true hvis vellykket, false hvis bruker ikke finnes eller ved feil</returns>
        public async Task<bool> UpdateBruker(BrukerRequest brukerRequest)
        {
            try
            {
                // Hent eksisterende bruker
                var eksisterendeBruker = await _repository.GetByEmail(brukerRequest.Email);
                if (eksisterendeBruker == null)
                {
                    _logger.LogWarning("Forsøk på å oppdatere ikke-eksisterende bruker med ID: {BrukerId}", brukerRequest.BrukerId);
                    return false;
                }

                // Oppdater kun de feltene som skal endres
                var oppdatertBruker = new Bruker
                {
                    BrukerId = brukerRequest.BrukerId,
                    Fornavn = brukerRequest.Fornavn,
                    Etternavn = brukerRequest.Etternavn,
                    Email = brukerRequest.Email,
                    Passord = eksisterendeBruker.Passord,
                    OpprettetDato = eksisterendeBruker.OpprettetDato
                };

                return await _repository.Update(oppdatertBruker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved oppdatering av bruker: {@BrukerRequest}", brukerRequest);
                return false;
            }
        }

        public async Task<bool> DeleteBruker(int brukerId)
        {   
            try
            {
                return await _repository.SoftDelete(brukerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved sletting av bruker med ID: {BrukerId}", brukerId);
                return false;
            }
        }


        public bool VerifyPassword(string password, string hashedPassword)
        {
            return _passwordService.VerifyPassword(password, hashedPassword);
        }
    }
} 