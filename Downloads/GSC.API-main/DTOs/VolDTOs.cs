using System.ComponentModel.DataAnnotations;

namespace GsC.API.DTOs
{
    public class VolDto
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime FlightDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public string Aircraft { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public int EstimatedPassengers { get; set; }
        public int ActualPassengers { get; set; }
        public TimeSpan Duration { get; set; }
        public string Season { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateVolDto
    {
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
    }

    public class UpdateVolDto
    {
        [StringLength(20)]
        public string? FlightNumber { get; set; }

        public DateTime? FlightDate { get; set; }

        public TimeSpan? DepartureTime { get; set; }

        public TimeSpan? ArrivalTime { get; set; }

        [StringLength(10)]
        public string? Aircraft { get; set; }

        [StringLength(100)]
        public string? Origin { get; set; }

        [StringLength(100)]
        public string? Destination { get; set; }

        [StringLength(50)]
        public string? Zone { get; set; }

        public int? EstimatedPassengers { get; set; }

        public int? ActualPassengers { get; set; }

        public TimeSpan? Duration { get; set; }

        [StringLength(20)]
        public string? Season { get; set; }
    }

    public class VolDetailsDto : VolDto
    {
        public PlanHebergementDto? PlanHebergement { get; set; }
        public List<BonCommandePrevisionnelDto> BonsCommandePrevisionnels { get; set; } = new();
        public List<BonLivraisonDto> BonsLivraison { get; set; } = new();
        public List<VolBoiteMedicaleDto> VolBoitesMedicales { get; set; } = new();
        public DossierVolDto? DossierVol { get; set; }
    }
}
