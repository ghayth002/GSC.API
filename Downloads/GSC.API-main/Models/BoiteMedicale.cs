using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public enum TypeBoiteMedicale
    {
        BoiteDoctor,
        BoitePharmacie,
        KitPremierSecours,
        BoiteUrgence
    }

    public enum StatusBoiteMedicale
    {
        Disponible,
        Assignee,
        EnMaintenance,
        Expiree
    }

    public class BoiteMedicale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public TypeBoiteMedicale Type { get; set; }

        [Required]
        public StatusBoiteMedicale Status { get; set; } = StatusBoiteMedicale.Disponible;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DateExpiration { get; set; }

        public DateTime DerniereMaintenance { get; set; }

        public DateTime? ProchaineMaintenance { get; set; }

        [StringLength(100)]
        public string? ResponsableMaintenance { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<BoiteMedicaleItem> Items { get; set; } = new List<BoiteMedicaleItem>();
        public virtual ICollection<VolBoiteMedicale> VolBoitesMedicales { get; set; } = new List<VolBoiteMedicale>();
    }

    public class BoiteMedicaleItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("BoiteMedicale")]
        public int BoiteMedicaleId { get; set; }

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

        // Navigation properties
        public virtual BoiteMedicale BoiteMedicale { get; set; } = null!;
    }

    public class VolBoiteMedicale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Vol")]
        public int VolId { get; set; }

        [Required]
        [ForeignKey("BoiteMedicale")]
        public int BoiteMedicaleId { get; set; }

        public DateTime DateAssignation { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? AssignePar { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }

        // Navigation properties
        public virtual Vol Vol { get; set; } = null!;
        public virtual BoiteMedicale BoiteMedicale { get; set; } = null!;
    }
}
