namespace KartverketGruppe5.Models.RequestModels
{
    public class InnmeldingRequest
    {
        public int? SaksbehandlerId { get; set; }
        public int? InnmelderBrukerId { get; set; }
        public string SortOrder { get; set; } = "date_desc";
        public string StatusFilter { get; set; } = "";
        public string FylkeFilter { get; set; } = "";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
} 