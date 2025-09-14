using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Services
{
    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BonCommandePrevisionnelDto> GenerateBcpFromMenusAsync(int volId, int userId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
            {
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé.");
            }

            // Récupérer les menus assignés au vol
            var assignedMenus = await _context.MenusPlanHebergement
                .Include(mph => mph.Menu)
                    .ThenInclude(m => m.MenuItems)
                        .ThenInclude(mi => mi.Article)
                .Include(mph => mph.PlanHebergement)
                .Where(mph => mph.PlanHebergement.VolId == volId && mph.Menu.IsActive)
                .ToListAsync();

            if (!assignedMenus.Any())
            {
                throw new InvalidOperationException("Aucun menu assigné à ce vol.");
            }

            // Calculer les quantités d'articles nécessaires
            var articleQuantities = await CalculateArticleQuantitiesAsync(volId);

            // Générer le numéro de BCP
            var numeroBcp = await GenerateBcpNumero(volId);

            // Créer le BCP
            var bcp = new BonCommandePrevisionnel
            {
                Numero = numeroBcp,
                VolId = volId,
                DateCommande = DateTime.UtcNow,
                Status = StatusBCP.Brouillon,
                Fournisseur = "Multiple",
                Commentaires = $"BCP généré automatiquement basé sur {assignedMenus.Count} menu(s) assigné(s)",
                CreatedAt = DateTime.UtcNow
            };

            _context.BonsCommandePrevisionnels.Add(bcp);
            await _context.SaveChangesAsync();

            // Créer les lignes de commande
            decimal montantTotal = 0;
            foreach (var (articleId, quantite) in articleQuantities)
            {
                var article = await _context.Articles.FindAsync(articleId);
                if (article != null && quantite > 0)
                {
                    var montantLigne = article.UnitPrice * quantite;
                    montantTotal += montantLigne;

                    var ligne = new BonCommandePrevisionnelLigne
                    {
                        BonCommandePrevisionnelId = bcp.Id,
                        ArticleId = articleId,
                        QuantiteCommandee = quantite,
                        PrixUnitaire = article.UnitPrice,
                        MontantLigne = montantLigne,
                        Commentaires = $"Généré depuis menus assignés"
                    };

                    _context.BonCommandePrevisionnelLignes.Add(ligne);
                }
            }

            // Mettre à jour le montant total du BCP
            bcp.MontantTotal = montantTotal;
            await _context.SaveChangesAsync();

            // Retourner le DTO
            var bcpDto = new BonCommandePrevisionnelDto
            {
                Id = bcp.Id,
                Numero = bcp.Numero,
                VolId = bcp.VolId,
                DateCommande = bcp.DateCommande,
                Status = bcp.Status,
                Fournisseur = bcp.Fournisseur,
                Commentaires = bcp.Commentaires,
                MontantTotal = bcp.MontantTotal,
                CreatedAt = bcp.CreatedAt,
                UpdatedAt = bcp.UpdatedAt
            };

            return bcpDto;
        }

        public async Task<Dictionary<int, int>> CalculateArticleQuantitiesAsync(int volId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
            {
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé.");
            }

            var articleQuantities = new Dictionary<int, int>();

            // Récupérer les menus assignés avec leurs articles
            var assignedMenus = await _context.MenusPlanHebergement
                .Include(mph => mph.Menu)
                    .ThenInclude(m => m.MenuItems)
                        .ThenInclude(mi => mi.Article)
                .Include(mph => mph.PlanHebergement)
                .Where(mph => mph.PlanHebergement.VolId == volId && mph.Menu.IsActive)
                .ToListAsync();

            foreach (var menuPlan in assignedMenus)
            {
                var menu = menuPlan.Menu;
                
                // Calculer le nombre de passagers pour ce type de menu
                var passengersForThisMenu = CalculatePassengersForMenu(vol, menu.TypePassager);

                foreach (var menuItem in menu.MenuItems)
                {
                    var articleId = menuItem.ArticleId;
                    var quantityPerPassenger = menuItem.Quantity;
                    var totalQuantity = quantityPerPassenger * passengersForThisMenu;

                    if (articleQuantities.ContainsKey(articleId))
                    {
                        articleQuantities[articleId] += totalQuantity;
                    }
                    else
                    {
                        articleQuantities[articleId] = totalQuantity;
                    }
                }
            }

            return articleQuantities;
        }

        public async Task<bool> ValidateMenuAssignmentAsync(int menuId, int volId)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            var vol = await _context.Vols.FindAsync(volId);

            if (menu == null || vol == null)
                return false;

            if (!menu.IsActive)
                return false;

            if (menu.FournisseurId == null)
                return false;

            // Vérifier les critères de compatibilité
            if (menu.Zone != null && menu.Zone != vol.Zone)
                return false;

            if (menu.Season != null && menu.Season != vol.Season)
                return false;

            return true;
        }

        public async Task<MenuStatisticsDto> GetMenuStatisticsAsync(int volId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
            {
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé.");
            }

            var assignedMenus = await _context.MenusPlanHebergement
                .Include(mph => mph.Menu)
                    .ThenInclude(m => m.MenuItems)
                        .ThenInclude(mi => mi.Article)
                .Include(mph => mph.PlanHebergement)
                .Where(mph => mph.PlanHebergement.VolId == volId && mph.Menu.IsActive)
                .ToListAsync();

            var statistics = new MenuStatisticsDto
            {
                TotalMenusAssigned = assignedMenus.Count,
                TotalArticles = assignedMenus.SelectMany(am => am.Menu.MenuItems).Select(mi => mi.ArticleId).Distinct().Count()
            };

            // Statistiques par type de passager
            foreach (var menuPlan in assignedMenus)
            {
                var typePassager = menuPlan.Menu.TypePassager;
                if (statistics.MenusByPassengerType.ContainsKey(typePassager))
                {
                    statistics.MenusByPassengerType[typePassager]++;
                }
                else
                {
                    statistics.MenusByPassengerType[typePassager] = 1;
                }
            }

            // Statistiques par type d'article et calcul du coût
            decimal totalCost = 0;
            var articleQuantities = await CalculateArticleQuantitiesAsync(volId);
            
            foreach (var (articleId, quantity) in articleQuantities)
            {
                var article = await _context.Articles.FindAsync(articleId);
                if (article != null)
                {
                    totalCost += article.UnitPrice * quantity;

                    if (statistics.ArticlesByType.ContainsKey(article.Type))
                    {
                        statistics.ArticlesByType[article.Type] += quantity;
                    }
                    else
                    {
                        statistics.ArticlesByType[article.Type] = quantity;
                    }
                }
            }

            statistics.EstimatedTotalCost = totalCost;

            return statistics;
        }

        private int CalculatePassengersForMenu(Vol vol, string typePassager)
        {
            // Estimation basée sur le type de passager et le nombre total de passagers
            var totalPassengers = vol.EstimatedPassengers > 0 ? vol.EstimatedPassengers : vol.ActualPassengers;

            return typePassager.ToLower() switch
            {
                "economy" => (int)(totalPassengers * 0.80), // 80% en économie
                "business" => (int)(totalPassengers * 0.15), // 15% en business
                "first" => (int)(totalPassengers * 0.05), // 5% en première
                _ => totalPassengers // Par défaut, tous les passagers
            };
        }

        private async Task<string> GenerateBcpNumero(int volId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            var prefix = $"BCP{vol!.FlightNumber}{vol.FlightDate:yyyyMMdd}";
            
            var lastBcp = await _context.BonsCommandePrevisionnels
                .Where(bcp => bcp.Numero.StartsWith(prefix))
                .OrderByDescending(bcp => bcp.Numero)
                .FirstOrDefaultAsync();

            if (lastBcp == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = lastBcp.Numero.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var number))
            {
                return $"{prefix}{(number + 1):D3}";
            }

            return $"{prefix}001";
        }
    }
}
