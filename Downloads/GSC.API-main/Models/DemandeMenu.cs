using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public enum StatusDemande
    {
        EnAttente,
        EnCours,
        Acceptee, // Acceptée par le fournisseur
        Refusee,  // Refusée par le fournisseur
        Completee,
        Annulee
    }

    public enum TypeDemande
    {
        Menu,
        Plat,
        MenuComplet
    }

    public class DemandeMenu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public TypeDemande Type { get; set; }

        [Required]
        public StatusDemande Status { get; set; } = StatusDemande.EnAttente;

        [Required]
        public DateTime DateDemande { get; set; } = DateTime.UtcNow;

        public DateTime? DateLimite { get; set; }

        public DateTime? DateReponse { get; set; }

        [Required]
        public int DemandeParUserId { get; set; }

        public int? AssigneAFournisseurId { get; set; }

        [StringLength(500)]
        public string? CommentairesAdmin { get; set; }

        [StringLength(500)]
        public string? CommentairesFournisseur { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(DemandeParUserId))]
        public virtual User DemandeParUser { get; set; } = null!;

        [ForeignKey(nameof(AssigneAFournisseurId))]
        public virtual User? AssigneAFournisseur { get; set; }

        public virtual ICollection<DemandePlat> DemandePlats { get; set; } = new List<DemandePlat>();
        public virtual ICollection<DemandeMenuReponse> Reponses { get; set; } = new List<DemandeMenuReponse>();
    }
}

