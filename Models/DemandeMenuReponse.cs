using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public class DemandeMenuReponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DemandeMenuId { get; set; }

        [Required]
        public int MenuProposedId { get; set; }

        [StringLength(200)]
        public string? NomMenuPropose { get; set; }

        [StringLength(1000)]
        public string? DescriptionMenuPropose { get; set; }

        public decimal? PrixTotal { get; set; }

        [StringLength(500)]
        public string? CommentairesFournisseur { get; set; }

        public bool IsAcceptedByAdmin { get; set; } = false;

        public DateTime DateProposition { get; set; } = DateTime.UtcNow;

        public DateTime? DateAcceptation { get; set; }

        [StringLength(500)]
        public string? CommentairesAcceptation { get; set; }

        // Navigation properties
        [ForeignKey(nameof(DemandeMenuId))]
        public virtual DemandeMenu DemandeMenu { get; set; } = null!;

        [ForeignKey(nameof(MenuProposedId))]
        public virtual Menu MenuProposed { get; set; } = null!;
    }
}

