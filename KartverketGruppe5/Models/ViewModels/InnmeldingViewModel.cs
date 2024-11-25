using KartverketGruppe5.Models.Helpers;
using System.ComponentModel.DataAnnotations;
namespace KartverketGruppe5.Models.ViewModels
{
    public class InnmeldingViewModel
    {
        public InnmeldingViewModel()
        {
            Beskrivelse = string.Empty;
            Status = "Ny";
            KommuneNavn = string.Empty;
            StatusClass = "bg-gray-100 text-gray-800";
        }

        public InnmeldingViewModel(Innmelding innmelding)
        {
            if (innmelding == null)
                throw new ArgumentNullException(nameof(innmelding));

            InnmeldingId = innmelding.InnmeldingId;
            BrukerId = innmelding.BrukerId;
            KommuneId = innmelding.KommuneId;
            LokasjonId = innmelding.LokasjonId;
            Beskrivelse = innmelding.Beskrivelse;
            Status = innmelding.Status;
            OpprettetDato = innmelding.OpprettetDato;
            OppdatertDato = innmelding.OppdatertDato;
            Kommentar = innmelding.Kommentar;
            SaksbehandlerId = innmelding.SaksbehandlerId;
            BildeSti = innmelding.BildeSti;
            
            KommuneNavn = innmelding.Kommune?.Navn ?? "";
            FylkeNavn = innmelding.Kommune?.Fylke?.Navn;
            SaksbehandlerNavn = innmelding.Saksbehandler?.Fornavn + " " + innmelding.Saksbehandler?.Etternavn;
            StatusClass = InnmeldingHelper.GetStatusClass(innmelding.Status);
        }

        public int InnmeldingId { get; set; }
        public int BrukerId { get; set; }
        public int KommuneId { get; set; }
        public int LokasjonId { get; set; }

        [Required(ErrorMessage = "Beskrivelse er påkrevd")]
        [StringLength(1000, ErrorMessage = "Beskrivelse kan ikke være lengre enn 1000 tegn")]
        public string Beskrivelse { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Kommentar kan ikke være lengre enn 1000 tegn")]
        public string? Kommentar { get; set; }
        public string Status { get; set; } = "Ny";
        public DateTime OpprettetDato { get; set; }
        public DateTime? OppdatertDato { get; set; }
        public string KommuneNavn { get; set; } = string.Empty;
        public string? FylkeNavn { get; set; }
        public string StatusClass { get; set; } = "bg-gray-100 text-gray-800";
        public int? SaksbehandlerId { get; set; }
        public string? SaksbehandlerNavn { get; set; }
        public IFormFile? Bilde { get; set; }
        public string? BildeSti { get; set; }
    }
} 