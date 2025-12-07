using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class BonCommandePrevisionnelDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public int VolId { get; set; }
        public DateTime DateCommande { get; set; }
        public StatusBCP Status { get; set; }
        public string? Fournisseur { get; set; }
        public decimal MontantTotal { get; set; }
        public string? Commentaires { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class CreateBonCommandePrevisionnelDto
    {
        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public int VolId { get; set; }

        [Required]
        public DateTime DateCommande { get; set; }

        [StringLength(100)]
        public string? Fournisseur { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }

        public List<CreateBonCommandePrevisionnelLigneDto> Lignes { get; set; } = new();
    }

    public class UpdateBonCommandePrevisionnelDto
    {
        [StringLength(50)]
        public string? Numero { get; set; }

        public DateTime? DateCommande { get; set; }

        public StatusBCP? Status { get; set; }

        [StringLength(100)]
        public string? Fournisseur { get; set; }

        [StringLength(500)]
        public string? Commentaires { get; set; }
    }

    public class BonCommandePrevisionnelDetailsDto : BonCommandePrevisionnelDto
    {
        public VolDto Vol { get; set; } = new();
        public List<BonCommandePrevisionnelLigneDto> Lignes { get; set; } = new();
        public List<EcartDto> Ecarts { get; set; } = new();
    }

    public class BonCommandePrevisionnelLigneDto
    {
        public int Id { get; set; }
        public int BonCommandePrevisionnelId { get; set; }
        public int ArticleId { get; set; }
        public int QuantiteCommandee { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal MontantLigne { get; set; }
        public string? Commentaires { get; set; }
        public ArticleDto Article { get; set; } = new();
    }

    public class CreateBonCommandePrevisionnelLigneDto
    {
        [Required]
        public int ArticleId { get; set; }

        [Required]
        public int QuantiteCommandee { get; set; }

        public decimal PrixUnitaire { get; set; }

        [StringLength(200)]
        public string? Commentaires { get; set; }
    }

    public class UpdateBonCommandePrevisionnelLigneDto
    {
        public int? QuantiteCommandee { get; set; }

        public decimal? PrixUnitaire { get; set; }

        [StringLength(200)]
        public string? Commentaires { get; set; }
    }
}
