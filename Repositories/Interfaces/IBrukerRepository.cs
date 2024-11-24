using KartverketGruppe5.Models;

namespace KartverketGruppe5.Repositories.Interfaces
{
    public interface IBrukerRepository
    {
        Task<Bruker?> GetByEmail(string email);
        Task<bool> Create(Bruker bruker);
        Task<bool> Delete(int brukerId);
        Task<bool> Update(Bruker bruker);
    }
} 