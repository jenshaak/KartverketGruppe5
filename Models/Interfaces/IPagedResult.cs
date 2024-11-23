namespace KartverketGruppe5.Models.Interfaces;

/// <summary>
/// Interface for håndtering av paginerte resultater.
/// Brukes for å standardisere paginering på tvers av applikasjonen.
/// </summary>
/// <typeparam name="T">Typen som skal pagineres (f.eks. Saksbehandler, Innmelding)</typeparam>
/// <remarks>
/// Dette interfaceet muliggjør:
/// - Konsistent paginering på tvers av ulike datatyper
/// - Enklere testing med mock-data
/// - Løs kobling mellom paginering-logikk og visning
/// </remarks>
public interface IPagedResult<T>
{
    /// <summary>
    /// Listen med elementer for gjeldende side
    /// </summary>
    List<T> Items { get; set; }

    /// <summary>
    /// Totalt antall elementer i hele resultatet
    /// </summary>
    int TotalItems { get; set; }

    /// <summary>
    /// Nåværende sidenummer (1-basert)
    /// </summary>
    int CurrentPage { get; set; }

    /// <summary>
    /// Antall elementer per side
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Totalt antall sider
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Indikerer om det finnes en forrige side
    /// </summary>
    bool HasPreviousPage { get; }

    /// <summary>
    /// Indikerer om det finnes en neste side
    /// </summary>
    bool HasNextPage { get; }

    /// <summary>
    /// Antall elementer som skal hoppes over
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Antall elementer som skal hentes
    /// </summary>
    int Take { get; }
} 