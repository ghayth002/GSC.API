using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class DemandeLivraisonComparisonDto
    {
        public int DemandeId { get; set; }
        public string DemandeNumero { get; set; } = string.Empty;
        public string DemandeTitre { get; set; } = string.Empty;
        public DateTime DateDemande { get; set; }
        
        public int? BonLivraisonId { get; set; }
        public string? BonLivraisonNumero { get; set; }
        public DateTime? DateLivraison { get; set; }
        public StatusBL? StatusLivraison { get; set; }
        
        public List<LigneComparisonDto> Lignes { get; set; } = new();
    }

    public class LigneComparisonDto
    {
        public int? ArticleId { get; set; }
        public string NomArticle { get; set; } = string.Empty;
        public string? CodeArticle { get; set; }
        
        // Demande info
        public int QuantiteDemandee { get; set; }
        public string? UniteDemandee { get; set; }
        
        // Livraison info
        public int QuantiteLivree { get; set; }
        public decimal? PrixUnitaire { get; set; }
        public decimal? MontantTotal { get; set; }
        
        // Comparison
        public int EcartQuantite { get; set; } // Livree - Demandee
        public string Status { get; set; } = "Conforme"; // Conforme, Manquant, Surplus, Partiel
    }
    
    public class AcceptDemandeDto
    {
        public int? DemandeId { get; set; }
        public string NumeroBL { get; set; } = string.Empty;
        public DateTime DateLivraison { get; set; }
        public string? Livreur { get; set; }
        public string? Commentaires { get; set; }
        public List<AcceptDemandeLigneDto> Lignes { get; set; } = new();
    }

    public class AcceptDemandeLigneDto
    {
        public int DemandePlatId { get; set; }
        public int? ArticleId { get; set; } // Nullable - l'article peut ne pas exister dans la base
        public string NomArticle { get; set; } = string.Empty;
        public int QuantiteLivree { get; set; }
        public decimal PrixUnitaire { get; set; }
    }
}
