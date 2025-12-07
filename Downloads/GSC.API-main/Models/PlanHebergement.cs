using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public class PlanHebergement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Vol")]
        public int VolId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Season { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string AircraftType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Zone { get; set; } = string.Empty;

        public TimeSpan FlightDuration { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Vol Vol { get; set; } = null!;
        public virtual ICollection<MenuPlanHebergement> MenusPlanHebergement { get; set; } = new List<MenuPlanHebergement>();
        public virtual ICollection<PlanHebergementArticle> PlanHebergementArticles { get; set; } = new List<PlanHebergementArticle>();
    }

    public class PlanHebergementArticle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("PlanHebergement")]
        public int PlanHebergementId { get; set; }

        [Required]
        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        [Required]
        public int QuantiteStandard { get; set; }

        [StringLength(50)]
        public string? TypePassager { get; set; }

        // Navigation properties
        public virtual PlanHebergement PlanHebergement { get; set; } = null!;
        public virtual Article Article { get; set; } = null!;
    }
}
