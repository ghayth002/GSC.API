using System.ComponentModel.DataAnnotations;

namespace GsC.API.DTOs
{
    public class PlanHebergementDto
    {
        public int Id { get; set; }
        public int VolId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Season { get; set; } = string.Empty;
        public string AircraftType { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public TimeSpan FlightDuration { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePlanHebergementDto
    {
        [Required]
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

        public List<CreatePlanHebergementArticleDto> Articles { get; set; } = new();
    }

    public class UpdatePlanHebergementDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Season { get; set; }

        [StringLength(10)]
        public string? AircraftType { get; set; }

        [StringLength(50)]
        public string? Zone { get; set; }

        public TimeSpan? FlightDuration { get; set; }

        public bool? IsActive { get; set; }
    }

    public class PlanHebergementDetailsDto : PlanHebergementDto
    {
        public VolDto Vol { get; set; } = new();
        public List<MenuDto> Menus { get; set; } = new();
        public List<PlanHebergementArticleDto> Articles { get; set; } = new();
    }

    public class PlanHebergementArticleDto
    {
        public int Id { get; set; }
        public int PlanHebergementId { get; set; }
        public int ArticleId { get; set; }
        public int QuantiteStandard { get; set; }
        public string? TypePassager { get; set; }
        public ArticleDto Article { get; set; } = new();
    }

    public class CreatePlanHebergementArticleDto
    {
        [Required]
        public int ArticleId { get; set; }

        [Required]
        public int QuantiteStandard { get; set; }

        [StringLength(50)]
        public string? TypePassager { get; set; }
    }

    public class UpdatePlanHebergementArticleDto
    {
        public int? QuantiteStandard { get; set; }

        [StringLength(50)]
        public string? TypePassager { get; set; }
    }
}
