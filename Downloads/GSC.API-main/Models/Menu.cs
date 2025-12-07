using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string TypePassager { get; set; } = string.Empty; // Economy, Business, First

        [StringLength(20)]
        public string? Season { get; set; }

        [StringLength(50)]
        public string? Zone { get; set; }

        public int? FournisseurId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Fournisseur? Fournisseur { get; set; }
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public virtual ICollection<MenuPlanHebergement> MenusPlanHebergement { get; set; } = new List<MenuPlanHebergement>();
        public virtual ICollection<DemandeMenuReponse> DemandeMenuReponses { get; set; } = new List<DemandeMenuReponse>();
    }

    public class MenuPlanHebergement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Menu")]
        public int MenuId { get; set; }

        [Required]
        [ForeignKey("PlanHebergement")]
        public int PlanHebergementId { get; set; }

        // Navigation properties
        public virtual Menu Menu { get; set; } = null!;
        public virtual PlanHebergement PlanHebergement { get; set; } = null!;
    }
}