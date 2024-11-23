using KartverketGruppe5.Models;

public interface IBrukerService
{
    Task<Bruker?> GetBrukerByEmail(string email);
    Task<bool> CreateBruker(Bruker bruker);
    Task<IEnumerable<Bruker>> GetAlleBrukere();
    Task<bool> OppdaterBruker(Bruker bruker);
    bool VerifyPassword(string password, string hashedPassword);
} 