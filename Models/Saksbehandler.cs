using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models
{
    public class Saksbehandler
    {
        [Key]
        public int SaksbehandlerId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Fornavn { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Etternavn { get; set; } = string.Empty;
        
        [Required]
        [StringLength(80)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(256)]
        public string Passord { get; set; } = string.Empty;
        
        public bool Admin { get; set; } = false;
        
        public DateTime OpprettetDato { get; set; }
    }
} 