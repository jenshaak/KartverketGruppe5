using KartverketGruppe5.Models;

namespace KartverketGruppe5.Services.Interfaces;

public interface IKommuneService
{
    Task<Kommune?> GetKommuneById(int kommuneId);
    Task<List<Kommune>> GetAllKommuner();
    Task<List<Kommune>> SearchKommuner(string searchTerm);
} 