using KartverketGruppe5.Models;

namespace KartverketGruppe5.Repositories.Interfaces
{
    public interface IKommuneRepository
    {
        Task<Kommune?> GetKommuneById(int kommuneId);
        Task<List<Kommune>> GetAllKommuner();
        Task<List<Kommune>> SearchKommuner(string searchTerm);
    }
} 