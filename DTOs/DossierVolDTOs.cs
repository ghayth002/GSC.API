using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class DossierVolDto
    {
        public int Id { get; set; }
        public int VolId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public StatusDossierVol Status { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateValidation { get; set; }
        public string? ValidePar { get; set; }
        public string? Resume { get; set; }
        public string? Commentaires { get; set; }
        public decimal CoutTotal { get; set; }
        public int NombreEcarts { get; set; }
        public decimal MontantEcarts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateDossierVolDto
    {
        [Required]
        public int VolId { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Resume { get; set; }

        [StringLength(1000)]
        public string? Commentaires { get; set; }
    }

    public class UpdateDossierVolDto
    {
        [StringLength(50)]
        public string? Numero { get; set; }

        public StatusDossierVol? Status { get; set; }

        public DateTime? DateValidation { get; set; }

        [StringLength(100)]
        public string? ValidePar { get; set; }

        [StringLength(1000)]
        public string? Resume { get; set; }

        [StringLength(1000)]
        public string? Commentaires { get; set; }
    }

    public class DossierVolDetailsDto : DossierVolDto
    {
        public VolDto Vol { get; set; } = new();
        public List<DossierVolDocumentDto> Documents { get; set; } = new();
    }

    public class DossierVolDocumentDto
    {
        public int Id { get; set; }
        public int DossierVolId { get; set; }
        public string NomDocument { get; set; } = string.Empty;
        public string TypeDocument { get; set; } = string.Empty;
        public string? CheminFichier { get; set; }
        public string? FormatFichier { get; set; }
        public long TailleFichier { get; set; }
        public DateTime DateAjout { get; set; }
        public string? AjoutePar { get; set; }
    }

    public class CreateDossierVolDocumentDto
    {
        [Required]
        public int DossierVolId { get; set; }

        [Required]
        [StringLength(200)]
        public string NomDocument { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TypeDocument { get; set; } = string.Empty;

        [StringLength(500)]
        public string? CheminFichier { get; set; }

        [StringLength(100)]
        public string? FormatFichier { get; set; }

        public long TailleFichier { get; set; }

        [StringLength(100)]
        public string? AjoutePar { get; set; }
    }

    public class UpdateDossierVolDocumentDto
    {
        [StringLength(200)]
        public string? NomDocument { get; set; }

        [StringLength(50)]
        public string? TypeDocument { get; set; }

        [StringLength(500)]
        public string? CheminFichier { get; set; }

        [StringLength(100)]
        public string? FormatFichier { get; set; }

        public long? TailleFichier { get; set; }

        [StringLength(100)]
        public string? AjoutePar { get; set; }
    }
}
