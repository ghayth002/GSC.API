using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class BonLivraisonDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public int? VolId { get; set; }
        public int? BonCommandePrevisionnelId { get; set; }
        public int? DemandeMenuId { get; set; }
        public int? DemandeId => DemandeMenuId; // Alias for frontend compatibility
        public DateTime DateLivraison { get; set; }
        public StatusBL Status { get; set; }
        public string Fournisseur { get; set; } = string.Empty;
        public string? Livreur { get; set; }
        public string? Commentaires { get; set; }
        public decimal MontantTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ValidatedBy { get; set; }
        public DateTime? ValidationDate { get; set; }
    }

    public class CreateBonLivraisonDto
    {
        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        public int? VolId { get; set; }
        
        public int? DemandeMenuId { get; set; }

        public int? BonCommandePrevisionnelId { get; set; }

        [Required]
        public DateTime DateLivraison { get; set; }

        [Required]
        [StringLength(100)]
        public string Fournisseur { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Livreur { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }

        public List<CreateBonLivraisonLigneDto> Lignes { get; set; } = new();
    }

    public class UpdateBonLivraisonDto
    {
        [StringLength(50)]
        public string? Numero { get; set; }

        public DateTime? DateLivraison { get; set; }

        public StatusBL? Status { get; set; }

        [StringLength(100)]
        public string? Fournisseur { get; set; }

        [StringLength(100)]
        public string? Livreur { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }
    }

    public class BonLivraisonDetailsDto : BonLivraisonDto
    {
        public VolDto Vol { get; set; } = new();
        public BonCommandePrevisionnelDto? BonCommandePrevisionnel { get; set; }
        public List<BonLivraisonLigneDto> Lignes { get; set; } = new();
        public List<EcartDto> Ecarts { get; set; } = new();
    }

    public class BonLivraisonLigneDto
    {
        public int Id { get; set; }
        public int BonLivraisonId { get; set; }
        public int? ArticleId { get; set; }
        public int QuantiteLivree { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal MontantLigne { get; set; }
        public string? Commentaires { get; set; }
        public ArticleDto? Article { get; set; }
    }

    public class CreateBonLivraisonLigneDto
    {
        public int? ArticleId { get; set; }

        [Required]
        public int QuantiteLivree { get; set; }

        public decimal PrixUnitaire { get; set; }

        [StringLength(200)]
        public string? Commentaires { get; set; }
    }

    public class UpdateBonLivraisonLigneDto
    {
        public int? QuantiteLivree { get; set; }

        public decimal? PrixUnitaire { get; set; }

        [StringLength(200)]
        public string? Commentaires { get; set; }
    }
}
