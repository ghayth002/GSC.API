using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class EcartDto
    {
        public int Id { get; set; }
        public int VolId { get; set; }
        public int ArticleId { get; set; }
        public int? BonCommandePrevisionnelId { get; set; }
        public int? BonLivraisonId { get; set; }
        public TypeEcart TypeEcart { get; set; }
        public StatusEcart Status { get; set; }
        public int QuantiteCommandee { get; set; }
        public int QuantiteLivree { get; set; }
        public int EcartQuantite { get; set; }
        public decimal PrixCommande { get; set; }
        public decimal PrixLivraison { get; set; }
        public decimal EcartMontant { get; set; }
        public string? Description { get; set; }
        public string? ActionCorrective { get; set; }
        public string? ResponsableTraitement { get; set; }
        public DateTime DateDetection { get; set; }
        public DateTime? DateResolution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateEcartDto
    {
        [Required]
        public int VolId { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public int? BonCommandePrevisionnelId { get; set; }

        public int? BonLivraisonId { get; set; }

        [Required]
        public TypeEcart TypeEcart { get; set; }

        public int QuantiteCommandee { get; set; }

        public int QuantiteLivree { get; set; }

        public decimal PrixCommande { get; set; }

        public decimal PrixLivraison { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? ActionCorrective { get; set; }

        [StringLength(100)]
        public string? ResponsableTraitement { get; set; }
    }

    public class UpdateEcartDto
    {
        public StatusEcart? Status { get; set; }

        public int? QuantiteCommandee { get; set; }

        public int? QuantiteLivree { get; set; }

        public decimal? PrixCommande { get; set; }

        public decimal? PrixLivraison { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? ActionCorrective { get; set; }

        [StringLength(100)]
        public string? ResponsableTraitement { get; set; }

        public DateTime? DateResolution { get; set; }
    }

    public class EcartDetailsDto : EcartDto
    {
        public VolDto Vol { get; set; } = new();
        public ArticleDto Article { get; set; } = new();
        public BonCommandePrevisionnelDto? BonCommandePrevisionnel { get; set; }
        public BonLivraisonDto? BonLivraison { get; set; }
    }
}
