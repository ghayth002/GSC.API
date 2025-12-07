using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Services
{
    public interface IGscBusinessService
    {
        /// <summary>
        /// Génère automatiquement un plan d'hébergement pour un vol basé sur les règles métier
        /// </summary>
        Task<PlanHebergement> GeneratePlanHebergementAsync(int volId, string season, string aircraftType, string zone, TimeSpan flightDuration);

        /// <summary>
        /// Génère automatiquement un BCP à partir d'un plan d'hébergement et des menus
        /// </summary>
        Task<BonCommandePrevisionnel> GenerateBcpFromPlanAsync(int volId, string? fournisseur = null);

        /// <summary>
        /// Effectue le rapprochement automatique entre BCP et BL et génère les écarts
        /// </summary>
        Task<List<Ecart>> ProcessBcpBlReconciliationAsync(int bonLivraisonId);

        /// <summary>
        /// Calcule les statistiques budgétaires pour une période donnée
        /// </summary>
        Task<BudgetStatistics> CalculateBudgetStatisticsAsync(DateTime dateDebut, DateTime dateFin, string? zone = null);

        /// <summary>
        /// Génère automatiquement un dossier de vol complet
        /// </summary>
        Task<DossierVol> GenerateCompleteDossierVolAsync(int volId);

        /// <summary>
        /// Valide la cohérence d'un vol (plan d'hébergement, menus, BCP, BL)
        /// </summary>
        Task<ValidationResult> ValidateVolConsistencyAsync(int volId);

        /// <summary>
        /// Calcule les coûts prévisionnels d'un vol
        /// </summary>
        Task<CostEstimate> CalculateVolCostEstimateAsync(int volId);

        /// <summary>
        /// Recommande des boîtes médicales pour un vol selon le type de vol et la destination
        /// </summary>
        Task<List<BoiteMedicale>> RecommendMedicalBoxesAsync(int volId);
    }

    public class BudgetStatistics
    {
        public int NombreVols { get; set; }
        public decimal MontantPrevu { get; set; }
        public decimal MontantReel { get; set; }
        public decimal EcartMontant { get; set; }
        public decimal PourcentageEcart { get; set; }
        public int NombreEcarts { get; set; }
        public decimal MontantEcarts { get; set; }
        public Dictionary<TypeArticle, decimal> CoutsByType { get; set; } = new();
        public Dictionary<string, decimal> CoutsByZone { get; set; } = new();
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class CostEstimate
    {
        public decimal EstimatedTotalCost { get; set; }
        public decimal CostPerPassenger { get; set; }
        public Dictionary<TypeArticle, decimal> CostsByType { get; set; } = new();
        public Dictionary<string, decimal> CostsByMenu { get; set; } = new();
        public List<string> CostBreakdown { get; set; } = new();
    }
}
