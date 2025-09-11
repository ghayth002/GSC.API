using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class BoiteMedicaleDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TypeBoiteMedicale Type { get; set; }
        public StatusBoiteMedicale Status { get; set; }
        public string? Description { get; set; }
        public DateTime DateExpiration { get; set; }
        public DateTime DerniereMaintenance { get; set; }
        public DateTime? ProchaineMaintenance { get; set; }
        public string? ResponsableMaintenance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateBoiteMedicaleDto
    {
        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public TypeBoiteMedicale Type { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DateExpiration { get; set; }

        public DateTime DerniereMaintenance { get; set; }

        public DateTime? ProchaineMaintenance { get; set; }

        [StringLength(100)]
        public string? ResponsableMaintenance { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateBoiteMedicaleItemDto> Items { get; set; } = new();
    }

    public class UpdateBoiteMedicaleDto
    {
        [StringLength(50)]
        public string? Numero { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        public TypeBoiteMedicale? Type { get; set; }

        public StatusBoiteMedicale? Status { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? DateExpiration { get; set; }

        public DateTime? DerniereMaintenance { get; set; }

        public DateTime? ProchaineMaintenance { get; set; }

        [StringLength(100)]
        public string? ResponsableMaintenance { get; set; }

        public bool? IsActive { get; set; }
    }

    public class BoiteMedicaleDetailsDto : BoiteMedicaleDto
    {
        public List<BoiteMedicaleItemDto> Items { get; set; } = new();
        public List<VolBoiteMedicaleDto> VolBoitesMedicales { get; set; } = new();
    }

    public class BoiteMedicaleItemDto
    {
        public int Id { get; set; }
        public int BoiteMedicaleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantite { get; set; }
        public string? Unite { get; set; }
        public DateTime? DateExpiration { get; set; }
        public string? Fabricant { get; set; }
        public string? NumeroLot { get; set; }
    }

    public class CreateBoiteMedicaleItemDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int Quantite { get; set; }

        [StringLength(50)]
        public string? Unite { get; set; }

        public DateTime? DateExpiration { get; set; }

        [StringLength(100)]
        public string? Fabricant { get; set; }

        [StringLength(50)]
        public string? NumeroLot { get; set; }
    }

    public class UpdateBoiteMedicaleItemDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? Quantite { get; set; }

        [StringLength(50)]
        public string? Unite { get; set; }

        public DateTime? DateExpiration { get; set; }

        [StringLength(100)]
        public string? Fabricant { get; set; }

        [StringLength(50)]
        public string? NumeroLot { get; set; }
    }

    public class VolBoiteMedicaleDto
    {
        public int Id { get; set; }
        public int VolId { get; set; }
        public int BoiteMedicaleId { get; set; }
        public DateTime DateAssignation { get; set; }
        public string? AssignePar { get; set; }
        public string? Commentaires { get; set; }
        public VolDto Vol { get; set; } = new();
        public BoiteMedicaleDto BoiteMedicale { get; set; } = new();
    }

    public class CreateVolBoiteMedicaleDto
    {
        [Required]
        public int VolId { get; set; }

        [Required]
        public int BoiteMedicaleId { get; set; }

        [StringLength(100)]
        public string? AssignePar { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }
    }

    public class UpdateVolBoiteMedicaleDto
    {
        [StringLength(100)]
        public string? AssignePar { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }
    }
}
