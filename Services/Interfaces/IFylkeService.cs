using KartverketGruppe5.Models;

namespace KartverketGruppe5.Services.Interfaces;

public interface IFylkeService
{
    Task<List<Fylke>> GetAllFylker();
} 