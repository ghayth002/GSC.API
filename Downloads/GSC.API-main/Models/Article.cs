using System.ComponentModel.DataAnnotations;

namespace GsC.API.Models
{
    public enum TypeArticle
    {
        Repas,
        Boisson,
        Consommable,
        SemiConsommable,
        MaterielDivers
    }

    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public TypeArticle Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        public int? FournisseurId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Fournisseur? Fournisseur { get; set; }
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public virtual ICollection<BonCommandePrevisionnelLigne> BonCommandePrevisionnelLignes { get; set; } = new List<BonCommandePrevisionnelLigne>();
        public virtual ICollection<BonLivraisonLigne> BonLivraisonLignes { get; set; } = new List<BonLivraisonLigne>();
        public virtual ICollection<PlanHebergementArticle> PlanHebergementArticles { get; set; } = new List<PlanHebergementArticle>();
        public virtual ICollection<Ecart> Ecarts { get; set; } = new List<Ecart>();
        public virtual ICollection<DemandePlat> DemandePlats { get; set; } = new List<DemandePlat>();
    }
}