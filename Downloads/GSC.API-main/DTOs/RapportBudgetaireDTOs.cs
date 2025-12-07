using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class RapportBudgetaireDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public TypeRapport TypeRapport { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public DateTime DateGeneration { get; set; }
        public string? GenerePar { get; set; }
        public int NombreVolsTotal { get; set; }
        public decimal MontantPrevu { get; set; }
        public decimal MontantReel { get; set; }
        public decimal EcartMontant { get; set; }
        public decimal PourcentageEcart { get; set; }
        public decimal CoutRepas { get; set; }
        public decimal CoutBoissons { get; set; }
        public decimal CoutConsommables { get; set; }
        public decimal CoutSemiConsommables { get; set; }
        public decimal CoutMaterielDivers { get; set; }
        public string? Commentaires { get; set; }
        public string? CheminFichier { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRapportBudgetaireDto
    {
        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public TypeRapport TypeRapport { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        [Required]
        public DateTime DateFin { get; set; }

        [StringLength(100)]
        public string? GenerePar { get; set; }

        [StringLength(2000)]
        public string? Commentaires { get; set; }
    }

    public class UpdateRapportBudgetaireDto
    {
        [StringLength(200)]
        public string? Titre { get; set; }

        public TypeRapport? TypeRapport { get; set; }

        public DateTime? DateDebut { get; set; }

        public DateTime? DateFin { get; set; }

        [StringLength(100)]
        public string? GenerePar { get; set; }

        [StringLength(2000)]
        public string? Commentaires { get; set; }

        [StringLength(500)]
        public string? CheminFichier { get; set; }
    }

    public class RapportBudgetaireDetailsDto : RapportBudgetaireDto
    {
        public List<RapportBudgetaireDetailDto> Details { get; set; } = new();
    }

    public class RapportBudgetaireDetailDto
    {
        public int Id { get; set; }
        public int RapportBudgetaireId { get; set; }
        public string? Categorie { get; set; }
        public string? Libelle { get; set; }
        public decimal MontantPrevu { get; set; }
        public decimal MontantReel { get; set; }
        public decimal Ecart { get; set; }
        public decimal PourcentageEcart { get; set; }
        public int Quantite { get; set; }
    }

    public class CreateRapportBudgetaireDetailDto
    {
        [Required]
        public int RapportBudgetaireId { get; set; }

        [StringLength(100)]
        public string? Categorie { get; set; }

        [StringLength(200)]
        public string? Libelle { get; set; }

        public decimal MontantPrevu { get; set; }

        public decimal MontantReel { get; set; }

        public int Quantite { get; set; }
    }

    public class UpdateRapportBudgetaireDetailDto
    {
        [StringLength(100)]
        public string? Categorie { get; set; }

        [StringLength(200)]
        public string? Libelle { get; set; }

        public decimal? MontantPrevu { get; set; }

        public decimal? MontantReel { get; set; }

        public int? Quantite { get; set; }
    }
}
