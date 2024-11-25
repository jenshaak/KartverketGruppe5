namespace KartverketGruppe5.Models.ViewModels
{
    /// <summary>
    /// ViewModel for feilmeldinger
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
