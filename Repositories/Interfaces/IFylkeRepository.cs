using KartverketGruppe5.Models;

namespace KartverketGruppe5.Repositories.Interfaces
{
    public interface IFylkeRepository
    {
        Task<List<Fylke>> GetAllFylker();
        Task<Fylke?> GetById(int fylkeId);  // God praksis å ha mulighet for å hente enkelt fylke
    }
} 