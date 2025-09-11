using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public enum TypeRapport
    {
        Quotidien,
        Hebdomadaire,
        Mensuel,
        Trimestriel,
        Annuel,
        Personnalise
    }

    public class RapportBudgetaire
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public TypeRapport TypeRapport { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        [Required]
        public DateTime DateFin { get; set; }

        public DateTime DateGeneration { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? GenerePar { get; set; }

        // Statistiques globales
        public int NombreVolsTotal { get; set; }

        public decimal MontantPrevu { get; set; }

        public decimal MontantReel { get; set; }

        public decimal EcartMontant { get; set; }

        public decimal PourcentageEcart { get; set; }

        // Statistiques par type d'article
        public decimal CoutRepas { get; set; }

        public decimal CoutBoissons { get; set; }

        public decimal CoutConsommables { get; set; }

        public decimal CoutSemiConsommables { get; set; }

        public decimal CoutMaterielDivers { get; set; }

        [StringLength(2000)]
        public string? Commentaires { get; set; }

        [StringLength(500)]
        public string? CheminFichier { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<RapportBudgetaireDetail> Details { get; set; } = new List<RapportBudgetaireDetail>();
    }

    public class RapportBudgetaireDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("RapportBudgetaire")]
        public int RapportBudgetaireId { get; set; }

        [StringLength(100)]
        public string? Categorie { get; set; } // Vol, Article, Fournisseur, etc.

        [StringLength(200)]
        public string? Libelle { get; set; }

        public decimal MontantPrevu { get; set; }

        public decimal MontantReel { get; set; }

        public decimal Ecart { get; set; }

        public decimal PourcentageEcart { get; set; }

        public int Quantite { get; set; }

        // Navigation properties
        public virtual RapportBudgetaire RapportBudgetaire { get; set; } = null!;
    }
}
