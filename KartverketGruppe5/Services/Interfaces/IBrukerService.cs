using KartverketGruppe5.Models;
using KartverketGruppe5.Models.RequestModels;
public interface IBrukerService
{
    Task<Bruker?> GetBrukerByEmail(string email);
    Task<bool> CreateBruker(Bruker bruker);

    Task<bool> UpdateBruker(BrukerRequest brukerRequest);
    Task<bool> DeleteBruker(int brukerId);
} 