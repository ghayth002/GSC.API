using GsC.API.Models;

namespace GsC.API.DTOs
{
    /// <summary>
    /// Vue d'ensemble des statistiques globales
    /// </summary>
    public class GlobalStatisticsDto
    {
        public int TotalDemandes { get; set; }
        public int DemandesEnAttente { get; set; }
        public int DemandesEnCours { get; set; }
        public int DemandesCompletees { get; set; }
        public int DemandesAnnulees { get; set; }
        public int TotalMenus { get; set; }
        public int MenusActifs { get; set; }
        public int TotalFournisseurs { get; set; }
        public int FournisseursActifs { get; set; }
        public int TotalVols { get; set; }
        public decimal TauxCompletionDemandes { get; set; }
    }

    /// <summary>
    /// Statistiques par statut de demande (pour Pie Chart)
    /// </summary>
    public class DemandeStatusStatDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Statistiques par type de demande (pour Pie Chart)
    /// </summary>
    public class DemandeTypeStatDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Performance des fournisseurs (pour Bar Chart)
    /// </summary>
    public class FournisseurPerformanceDto
    {
        public int FournisseurId { get; set; }
        public string FournisseurName { get; set; } = string.Empty;
        public int TotalDemandesAssignees { get; set; }
        public int DemandesAcceptees { get; set; }
        public int DemandesRefusees { get; set; }
        public int DemandesCompletees { get; set; }
        public decimal TauxAcceptation { get; set; }
        public decimal TauxCompletion { get; set; }
        public decimal DelaiMoyenReponse { get; set; } // En heures
    }

    /// <summary>
    /// Tendances temporelles (pour Line Chart)
    /// </summary>
    public class TrendDataDto
    {
        public string Period { get; set; } = string.Empty; // "2024-01", "2024-W01", "2024-01-15"
        public int DemandesCreees { get; set; }
        public int DemandesCompletees { get; set; }
        public int MenusCrees { get; set; }
    }

    /// <summary>
    /// Statistiques des menus pour analytics
    /// </summary>
    public class MenuAnalyticsDto
    {
        public int TotalMenus { get; set; }
        public int MenusActifs { get; set; }
        public int MenusParFournisseur { get; set; }
        public Dictionary<string, int> MenusParTypePassager { get; set; } = new();
        public Dictionary<string, int> MenusParSaison { get; set; } = new();
    }

    /// <summary>
    /// Rapport complet pour export PDF
    /// </summary>
    public class AnalyticsReportDto
    {
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public GlobalStatisticsDto GlobalStats { get; set; } = new();
        public List<DemandeStatusStatDto> StatusDistribution { get; set; } = new();
        public List<DemandeTypeStatDto> TypeDistribution { get; set; } = new();
        public List<FournisseurPerformanceDto> TopFournisseurs { get; set; } = new();
        public List<TrendDataDto> MonthlyTrends { get; set; } = new();
        public MenuAnalyticsDto MenuStats { get; set; } = new();
    }

    /// <summary>
    /// Paramètres pour filtrer les analytics
    /// </summary>
    public class AnalyticsFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? FournisseurId { get; set; }
        public StatusDemande? Status { get; set; }
        public TypeDemande? Type { get; set; }
    }
}
