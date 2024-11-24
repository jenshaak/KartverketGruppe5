using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models.RequestModels
{
    public class BrukerRequest
    {
        [Key]
        public int BrukerId { get; set; }
        
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
        
        [StringLength(256)]
        public string? Passord { get; set; }
    }
}