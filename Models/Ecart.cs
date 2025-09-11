using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public enum TypeEcart
    {
        QuantiteSuperieure,
        QuantiteInferieure,
        ArticleManquant,
        ArticleEnPlus,
        PrixDifferent
    }

    public enum StatusEcart
    {
        EnAttente,
        EnCours,
        Resolu,
        Accepte,
        Rejete
    }

    public class Ecart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Vol")]
        public int VolId { get; set; }

        [Required]
        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        [ForeignKey("BonCommandePrevisionnel")]
        public int? BonCommandePrevisionnelId { get; set; }

        [ForeignKey("BonLivraison")]
        public int? BonLivraisonId { get; set; }

        [Required]
        public TypeEcart TypeEcart { get; set; }

        [Required]
        public StatusEcart Status { get; set; } = StatusEcart.EnAttente;

        public int QuantiteCommandee { get; set; }

        public int QuantiteLivree { get; set; }

        public int EcartQuantite { get; set; }

        public decimal PrixCommande { get; set; }

        public decimal PrixLivraison { get; set; }

        public decimal EcartMontant { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? ActionCorrective { get; set; }

        [StringLength(100)]
        public string? ResponsableTraitement { get; set; }

        public DateTime DateDetection { get; set; } = DateTime.UtcNow;

        public DateTime? DateResolution { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Vol Vol { get; set; } = null!;
        public virtual Article Article { get; set; } = null!;
        public virtual BonCommandePrevisionnel? BonCommandePrevisionnel { get; set; }
        public virtual BonLivraison? BonLivraison { get; set; }
    }
}
