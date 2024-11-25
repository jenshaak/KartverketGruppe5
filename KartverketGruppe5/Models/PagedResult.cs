using KartverketGruppe5.Models.Interfaces;

namespace KartverketGruppe5.Models
{
    /// <summary>
    /// Klasse for å håndtere paginering av saksbehandlere og innmeldinger i tabellene
    /// </summary>
    public class PagedResult<T> : IPagedResult<T>
    {
        public const string DefaultSortOrder = "date_desc";
        public const int DefaultPage = 1;
        private const int MaxPageSize = 50;
        private const int DefaultPageSize = 10;
        
        public required List<T> Items { get; set; }
        public int TotalItems { get; set; }
        
        private int _currentPage = DefaultPage;
        public int CurrentPage
        {
            get => _currentPage;
            set => _currentPage = Math.Max(1, value);
        }
        
        private int _pageSize = DefaultPageSize;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Min(Math.Max(1, value), MaxPageSize);
        }

        // Beregner slik at f.eks. 63 items, med 10 items per side blir 7 sider
        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        
        // Hjelpemetoder for paginering
        public int Skip => (CurrentPage - 1) * PageSize;
        public int Take => PageSize;
    }
} 