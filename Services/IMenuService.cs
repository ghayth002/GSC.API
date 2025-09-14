using GsC.API.DTOs;
using GsC.API.Models;

namespace GsC.API.Services
{
    public interface IMenuService
    {
        /// <summary>
        /// Génère automatiquement un BCP basé sur les menus assignés à un vol
        /// </summary>
        Task<BonCommandePrevisionnelDto> GenerateBcpFromMenusAsync(int volId, int userId);

        /// <summary>
        /// Calcule les quantités nécessaires pour un vol basé sur les menus et le nombre de passagers
        /// </summary>
        Task<Dictionary<int, int>> CalculateArticleQuantitiesAsync(int volId);

        /// <summary>
        /// Valide qu'un menu peut être assigné à un vol
        /// </summary>
        Task<bool> ValidateMenuAssignmentAsync(int menuId, int volId);

        /// <summary>
        /// Récupère les statistiques des menus pour un vol
        /// </summary>
        Task<MenuStatisticsDto> GetMenuStatisticsAsync(int volId);
    }

    public class MenuStatisticsDto
    {
        public int TotalMenusAssigned { get; set; }
        public int TotalArticles { get; set; }
        public decimal EstimatedTotalCost { get; set; }
        public Dictionary<string, int> MenusByPassengerType { get; set; } = new();
        public Dictionary<TypeArticle, int> ArticlesByType { get; set; } = new();
    }
}