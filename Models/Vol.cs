using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public class Vol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public DateTime FlightDate { get; set; }

        [Required]
        public TimeSpan DepartureTime { get; set; }

        [Required]
        public TimeSpan ArrivalTime { get; set; }

        [Required]
        [StringLength(10)]
        public string Aircraft { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Origin { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Zone { get; set; } = string.Empty;

        public int EstimatedPassengers { get; set; }

        public int ActualPassengers { get; set; }

        public TimeSpan Duration { get; set; }

        [StringLength(20)]
        public string Season { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual PlanHebergement? PlanHebergement { get; set; }
        public virtual ICollection<BonCommandePrevisionnel> BonsCommandePrevisionnels { get; set; } = new List<BonCommandePrevisionnel>();
        public virtual ICollection<BonLivraison> BonsLivraison { get; set; } = new List<BonLivraison>();
        public virtual ICollection<VolBoiteMedicale> VolBoitesMedicales { get; set; } = new List<VolBoiteMedicale>();
        public virtual DossierVol? DossierVol { get; set; }
    }
}
