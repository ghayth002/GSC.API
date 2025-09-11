using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public enum StatusDossierVol
    {
        EnPreparation,
        Complete,
        Valide,
        Archive
    }

    public class DossierVol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Vol")]
        public int VolId { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public StatusDossierVol Status { get; set; } = StatusDossierVol.EnPreparation;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateValidation { get; set; }

        [StringLength(100)]
        public string? ValidePar { get; set; }

        [StringLength(1000)]
        public string? Resume { get; set; }

        [StringLength(1000)]
        public string? Commentaires { get; set; }

        public decimal CoutTotal { get; set; }

        public int NombreEcarts { get; set; }

        public decimal MontantEcarts { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Vol Vol { get; set; } = null!;
        public virtual ICollection<DossierVolDocument> Documents { get; set; } = new List<DossierVolDocument>();
    }

    public class DossierVolDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("DossierVol")]
        public int DossierVolId { get; set; }

        [Required]
        [StringLength(200)]
        public string NomDocument { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TypeDocument { get; set; } = string.Empty; // BCP, BL, Menu, Ecart, etc.

        [StringLength(500)]
        public string? CheminFichier { get; set; }

        [StringLength(100)]
        public string? FormatFichier { get; set; }

        public long TailleFichier { get; set; }

        public DateTime DateAjout { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? AjoutePar { get; set; }

        // Navigation properties
        public virtual DossierVol DossierVol { get; set; } = null!;
    }
}
