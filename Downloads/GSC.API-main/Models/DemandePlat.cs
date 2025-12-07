using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public class DemandePlat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DemandeMenuId { get; set; }

        [Required]
        [StringLength(200)]
        public string NomPlatSouhaite { get; set; } = string.Empty;

        [StringLength(500)]
        public string? DescriptionSouhaitee { get; set; }

        [Required]
        public TypeArticle TypePlat { get; set; }

        [StringLength(50)]
        public string? UniteSouhaitee { get; set; }

        public decimal? PrixMaximal { get; set; }

        public int? QuantiteEstimee { get; set; }

        [StringLength(200)]
        public string? SpecificationsSpeciales { get; set; }

        public bool IsObligatoire { get; set; } = false;

        public StatusDemande Status { get; set; } = StatusDemande.EnAttente;

        public int? ArticleProposedId { get; set; }

        [StringLength(500)]
        public string? CommentairesFournisseur { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(DemandeMenuId))]
        public virtual DemandeMenu DemandeMenu { get; set; } = null!;

        [ForeignKey(nameof(ArticleProposedId))]
        public virtual Article? ArticleProposed { get; set; }
    }
}

