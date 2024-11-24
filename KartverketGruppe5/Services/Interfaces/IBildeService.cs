namespace KartverketGruppe5.Services.Interfaces;

public interface IBildeService
{
    /// <summary>
    /// Lagrer et opplastet bilde for en innmelding
    /// </summary>
    /// <param name="bilde">Bildefilen som skal lagres</param>
    /// <param name="innmeldingId">ID-en til innmeldingen bildet tilhører</param>
    /// <returns>Relativ sti til lagret bilde, eller null hvis ingen bilde ble lastet opp</returns>
    Task<string?> LagreBilde(IFormFile? bilde, int innmeldingId);
} 