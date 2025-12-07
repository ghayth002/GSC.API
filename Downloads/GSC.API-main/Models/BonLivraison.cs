using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public enum StatusBL
    {
        EnAttente,
        Recu,
        Valide,
        Rejete
    }

    public class BonLivraison
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [ForeignKey("Vol")]
        public int? VolId { get; set; }

        [ForeignKey("BonCommandePrevisionnel")]
        public int? BonCommandePrevisionnelId { get; set; }

        [ForeignKey("DemandeMenu")]
        public int? DemandeMenuId { get; set; }

        [Required]
        public DateTime DateLivraison { get; set; }

        [Required]
        public StatusBL Status { get; set; } = StatusBL.EnAttente;

        [Required]
        [StringLength(100)]
        public string Fournisseur { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Livreur { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }

        public decimal MontantTotal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? ValidatedBy { get; set; }

        public DateTime? ValidationDate { get; set; }

        // Navigation properties
        public virtual Vol? Vol { get; set; }
        public virtual BonCommandePrevisionnel? BonCommandePrevisionnel { get; set; }
        public virtual DemandeMenu? DemandeMenu { get; set; }
        public virtual ICollection<BonLivraisonLigne> Lignes { get; set; } = new List<BonLivraisonLigne>();
        public virtual ICollection<Ecart> Ecarts { get; set; } = new List<Ecart>();
    }

    public class BonLivraisonLigne
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("BonLivraison")]
        public int BonLivraisonId { get; set; }

        [ForeignKey("Article")]
        public int? ArticleId { get; set; } // Nullable - peut être null si plat non lié à un article

        public int? DemandePlatId { get; set; }

        [StringLength(200)]
        public string? NomArticle { get; set; }

        [Required]
        public int QuantiteLivree { get; set; }

        public decimal PrixUnitaire { get; set; }

        public decimal MontantLigne { get; set; }

        [StringLength(200)]
        public string? Commentaires { get; set; }

        // Navigation properties
        public virtual BonLivraison BonLivraison { get; set; } = null!;
        public virtual Article? Article { get; set; }
    }
}
