using System.Data;

namespace KartverketGruppe5.Repositories.Interfaces
{
    public interface IKommunePopulateRepository
    {
        Task<List<string>> GetExistingKommuneNumbers();
        Task<List<string>> GetExistingFylkeNumbers();
        Task<bool> CheckFylkeExists(string fylkeNumber, string navn, IDbTransaction transaction);
        Task<int> InsertOrUpdateFylke(string fylkeNumber, string navn, IDbTransaction transaction);
        Task<bool> CheckKommuneExists(string kommuneNumber, string navn, int fylkeId, IDbTransaction transaction);
        Task InsertOrUpdateKommune(string kommuneNumber, int fylkeId, string navn, IDbTransaction transaction);
        Task<IDbConnection> CreateConnection();
    }
} 