using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public enum StatusBCP
    {
        Brouillon,
        Envoye,
        Confirme,
        Annule
    }

    public class BonCommandePrevisionnel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Vol")]
        public int VolId { get; set; }

        [Required]
        public DateTime DateCommande { get; set; }

        [Required]
        public StatusBCP Status { get; set; } = StatusBCP.Brouillon;

        [StringLength(100)]
        public string? Fournisseur { get; set; }

        public decimal MontantTotal { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation properties
        public virtual Vol Vol { get; set; } = null!;
        public virtual ICollection<BonCommandePrevisionnelLigne> Lignes { get; set; } = new List<BonCommandePrevisionnelLigne>();
        public virtual ICollection<Ecart> Ecarts { get; set; } = new List<Ecart>();
    }

    public class BonCommandePrevisionnelLigne
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("BonCommandePrevisionnel")]
        public int BonCommandePrevisionnelId { get; set; }

        [Required]
        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        [Required]
        public int QuantiteCommandee { get; set; }

        public decimal PrixUnitaire { get; set; }

        public decimal MontantLigne { get; set; }

        [StringLength(200)]
        public string? Commentaires { get; set; }

        // Navigation properties
        public virtual BonCommandePrevisionnel BonCommandePrevisionnel { get; set; } = null!;
        public virtual Article Article { get; set; } = null!;
    }
}
