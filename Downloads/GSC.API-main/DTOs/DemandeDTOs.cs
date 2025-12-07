using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    // DTOs pour DemandeMenu
    public class DemandeMenuDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TypeDemande Type { get; set; }
        public StatusDemande Status { get; set; }
        public DateTime DateDemande { get; set; }
        public DateTime? DateLimite { get; set; }
        public DateTime? DateReponse { get; set; }
        public int DemandeParUserId { get; set; }
        public string DemandeParUserName { get; set; } = string.Empty;
        public int? AssigneAFournisseurId { get; set; }
        public string? AssigneAFournisseurName { get; set; }
        public string? CommentairesAdmin { get; set; }
        public string? CommentairesFournisseur { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<DemandePlatDto> DemandePlats { get; set; } = new();
        public List<DemandeMenuReponseDto> Reponses { get; set; } = new();
    }

    public class CreateDemandeMenuDto
    {
        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public TypeDemande Type { get; set; }

        public DateTime? DateLimite { get; set; }

        [StringLength(500)]
        public string? CommentairesAdmin { get; set; }

        public int? AssigneAFournisseurId { get; set; }

        public List<CreateDemandePlatDto> DemandePlats { get; set; } = new();
    }

    public class UpdateDemandeMenuDto
    {
        [StringLength(200)]
        public string? Titre { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public TypeDemande? Type { get; set; }

        public StatusDemande? Status { get; set; }

        public DateTime? DateLimite { get; set; }

        public int? AssigneAFournisseurId { get; set; }

        [StringLength(500)]
        public string? CommentairesAdmin { get; set; }

        [StringLength(500)]
        public string? CommentairesFournisseur { get; set; }
    }

    // DTOs pour DemandePlat
    public class DemandePlatDto
    {
        public int Id { get; set; }
        public int DemandeMenuId { get; set; }
        public string NomPlatSouhaite { get; set; } = string.Empty;
        public string? DescriptionSouhaitee { get; set; }
        public TypeArticle TypePlat { get; set; }
        public string? UniteSouhaitee { get; set; }
        public decimal? PrixMaximal { get; set; }
        public int? QuantiteEstimee { get; set; }
        public string? SpecificationsSpeciales { get; set; }
        public bool IsObligatoire { get; set; }
        public StatusDemande Status { get; set; }
        public int? ArticleProposedId { get; set; }
        public ArticleDto? ArticleProposed { get; set; }
        public string? CommentairesFournisseur { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateDemandePlatDto
    {
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
    }

    public class UpdateDemandePlatDto
    {
        [StringLength(200)]
        public string? NomPlatSouhaite { get; set; }

        [StringLength(500)]
        public string? DescriptionSouhaitee { get; set; }

        public TypeArticle? TypePlat { get; set; }

        [StringLength(50)]
        public string? UniteSouhaitee { get; set; }

        public decimal? PrixMaximal { get; set; }

        public int? QuantiteEstimee { get; set; }

        [StringLength(200)]
        public string? SpecificationsSpeciales { get; set; }

        public bool? IsObligatoire { get; set; }

        public StatusDemande? Status { get; set; }

        public int? ArticleProposedId { get; set; }

        [StringLength(500)]
        public string? CommentairesFournisseur { get; set; }
    }

    // DTOs pour DemandeMenuReponse
    public class DemandeMenuReponseDto
    {
        public int Id { get; set; }
        public int DemandeMenuId { get; set; }
        public int MenuProposedId { get; set; }
        public MenuDto MenuProposed { get; set; } = new();
        public string? NomMenuPropose { get; set; }
        public string? DescriptionMenuPropose { get; set; }
        public decimal? PrixTotal { get; set; }
        public string? CommentairesFournisseur { get; set; }
        public bool IsAcceptedByAdmin { get; set; }
        public DateTime DateProposition { get; set; }
        public DateTime? DateAcceptation { get; set; }
        public string? CommentairesAcceptation { get; set; }
    }

    public class CreateDemandeMenuReponseDto
    {
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
    }

    public class UpdateDemandeMenuReponseDto
    {
        [StringLength(200)]
        public string? NomMenuPropose { get; set; }

        [StringLength(1000)]
        public string? DescriptionMenuPropose { get; set; }

        public decimal? PrixTotal { get; set; }

        [StringLength(500)]
        public string? CommentairesFournisseur { get; set; }

        public bool? IsAcceptedByAdmin { get; set; }

        [StringLength(500)]
        public string? CommentairesAcceptation { get; set; }
    }

    // DTOs pour Fournisseur
    public class FournisseurDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPerson { get; set; }
        public string? Siret { get; set; }
        public string? NumeroTVA { get; set; }
        public string? Specialites { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateFournisseurDto
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserFirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserLastName { get; set; } = string.Empty;

        // Password is now auto-generated

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? ContactEmail { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(50)]
        public string? Siret { get; set; }

        [StringLength(50)]
        public string? NumeroTVA { get; set; }

        [StringLength(500)]
        public string? Specialites { get; set; }
    }

    public class UpdateFournisseurDto
    {
        [StringLength(100)]
        public string? CompanyName { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? ContactEmail { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(50)]
        public string? Siret { get; set; }

        [StringLength(50)]
        public string? NumeroTVA { get; set; }

        [StringLength(500)]
        public string? Specialites { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsVerified { get; set; }
    }

    // DTO pour assigner une demande à un fournisseur
    public class AssignDemandeToFournisseurDto
    {
        [Required]
        public int FournisseurId { get; set; }

        [StringLength(500)]
        public string? CommentairesAdmin { get; set; }

        public DateTime? DateLimite { get; set; }
    }

    // DTO pour refuser une demande (ajouté pour le nouveau système de livraison)
    public class RefuseDemandeDto
    {
        [Required]
        [StringLength(500)]
        public string Raison { get; set; } = string.Empty;
    }
}
