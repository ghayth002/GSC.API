using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Services
{
    public class GscBusinessService : IGscBusinessService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GscBusinessService> _logger;

        public GscBusinessService(ApplicationDbContext context, ILogger<GscBusinessService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PlanHebergement> GeneratePlanHebergementAsync(int volId, string season, string aircraftType, string zone, TimeSpan flightDuration)
        {
            _logger.LogInformation($"Génération du plan d'hébergement pour le vol {volId}");

            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé");

            // Vérifier s'il existe déjà un plan pour ce vol
            var existingPlan = await _context.PlansHebergement.FirstOrDefaultAsync(p => p.VolId == volId);
            if (existingPlan != null)
                throw new InvalidOperationException($"Un plan d'hébergement existe déjà pour le vol {volId}");

            var plan = new PlanHebergement
            {
                VolId = volId,
                Name = $"Plan {vol.FlightNumber} - {vol.FlightDate:dd/MM/yyyy}",
                Description = $"Plan d'hébergement généré automatiquement",
                Season = season,
                AircraftType = aircraftType,
                Zone = zone,
                FlightDuration = flightDuration,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.PlansHebergement.Add(plan);
            await _context.SaveChangesAsync();

            // Ajouter les articles standard selon les règles métier
            await AddStandardArticlesToPlan(plan, vol);

            _logger.LogInformation($"Plan d'hébergement {plan.Id} créé pour le vol {volId}");
            return plan;
        }

        public async Task<BonCommandePrevisionnel> GenerateBcpFromPlanAsync(int volId, string? fournisseur = null)
        {
            _logger.LogInformation($"Génération du BCP pour le vol {volId}");

            var vol = await _context.Vols
                .Include(v => v.PlanHebergement)
                    .ThenInclude(ph => ph!.PlanHebergementArticles)
                        .ThenInclude(pha => pha.Article)
                .Include(v => v.PlanHebergement)
                    .ThenInclude(ph => ph!.MenusPlanHebergement)
                        .ThenInclude(mph => mph.Menu)
                            .ThenInclude(m => m.MenuItems)
                                .ThenInclude(mi => mi.Article)
                .FirstOrDefaultAsync(v => v.Id == volId);

            if (vol == null)
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé");

            if (vol.PlanHebergement == null)
                throw new InvalidOperationException($"Aucun plan d'hébergement trouvé pour le vol {volId}");

            // Générer un numéro unique
            var numero = await GenerateUniqueBcpNumber(vol);

            var bcp = new BonCommandePrevisionnel
            {
                Numero = numero,
                VolId = volId,
                DateCommande = DateTime.UtcNow,
                Status = StatusBCP.Brouillon,
                Fournisseur = fournisseur,
                MontantTotal = 0,
                Commentaires = $"BCP généré automatiquement le {DateTime.Now:dd/MM/yyyy}",
                CreatedAt = DateTime.UtcNow
            };

            _context.BonsCommandePrevisionnels.Add(bcp);
            await _context.SaveChangesAsync();

            // Calculer les quantités et créer les lignes
            var articleQuantities = await CalculateArticleQuantities(vol);
            decimal montantTotal = 0;

            foreach (var (articleId, quantity) in articleQuantities)
            {
                var article = await _context.Articles.FindAsync(articleId);
                if (article == null) continue;

                var montantLigne = quantity * article.UnitPrice;
                var ligne = new BonCommandePrevisionnelLigne
                {
                    BonCommandePrevisionnelId = bcp.Id,
                    ArticleId = articleId,
                    QuantiteCommandee = quantity,
                    PrixUnitaire = article.UnitPrice,
                    MontantLigne = montantLigne,
                    Commentaires = "Généré automatiquement"
                };

                _context.BonCommandePrevisionnelLignes.Add(ligne);
                montantTotal += montantLigne;
            }

            bcp.MontantTotal = montantTotal;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"BCP {bcp.Numero} créé pour le vol {volId} avec un montant de {montantTotal:C}");
            return bcp;
        }

        public async Task<List<Ecart>> ProcessBcpBlReconciliationAsync(int bonLivraisonId)
        {
            _logger.LogInformation($"Traitement du rapprochement pour le BL {bonLivraisonId}");

            var bonLivraison = await _context.BonsLivraison
                .Include(bl => bl.BonCommandePrevisionnel)
                    .ThenInclude(bcp => bcp!.Lignes)
                        .ThenInclude(l => l.Article)
                .Include(bl => bl.Lignes)
                    .ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(bl => bl.Id == bonLivraisonId);

            if (bonLivraison == null)
                throw new ArgumentException($"Bon de livraison avec l'ID {bonLivraisonId} non trouvé");

            if (bonLivraison.BonCommandePrevisionnel == null)
            {
                _logger.LogWarning($"Aucun BCP associé au BL {bonLivraisonId}");
                return new List<Ecart>();
            }

            var ecarts = new List<Ecart>();
            var bcpArticles = bonLivraison.BonCommandePrevisionnel.Lignes.ToDictionary(l => l.ArticleId, l => l);
            var blArticles = bonLivraison.Lignes.ToDictionary(l => l.ArticleId, l => l);

            // Analyser les écarts pour les articles commandés
            foreach (var bcpLigne in bonLivraison.BonCommandePrevisionnel.Lignes)
            {
                if (blArticles.TryGetValue(bcpLigne.ArticleId, out var blLigne))
                {
                    var ecart = AnalyzeArticleEcart(bonLivraison, bcpLigne, blLigne);
                    if (ecart != null)
                        ecarts.Add(ecart);
                }
                else
                {
                    // Article commandé mais pas livré
                    ecarts.Add(CreateMissingArticleEcart(bonLivraison, bcpLigne));
                }
            }

            // Analyser les articles livrés mais non commandés
            foreach (var blLigne in bonLivraison.Lignes)
            {
                // Skip if no ArticleId
                if (!blLigne.ArticleId.HasValue) continue;

                if (!bcpArticles.ContainsKey(blLigne.ArticleId.Value))
                {
                    ecarts.Add(CreateExtraArticleEcart(bonLivraison, blLigne));
                }
            }

            if (ecarts.Any())
            {
                _context.Ecarts.AddRange(ecarts);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{ecarts.Count} écarts créés pour le BL {bonLivraisonId}");
            }

            return ecarts;
        }

        public async Task<BudgetStatistics> CalculateBudgetStatisticsAsync(DateTime dateDebut, DateTime dateFin, string? zone = null)
        {
            _logger.LogInformation($"Calcul des statistiques budgétaires du {dateDebut:dd/MM/yyyy} au {dateFin:dd/MM/yyyy}");

            var query = _context.Vols.AsQueryable();
            if (!string.IsNullOrEmpty(zone))
                query = query.Where(v => v.Zone == zone);

            var vols = await query
                .Include(v => v.BonsCommandePrevisionnels)
                    .ThenInclude(bcp => bcp.Lignes)
                        .ThenInclude(l => l.Article)
                .Include(v => v.BonsLivraison)
                    .ThenInclude(bl => bl.Lignes)
                        .ThenInclude(l => l.Article)
                .Where(v => v.FlightDate >= dateDebut && v.FlightDate <= dateFin)
                .ToListAsync();

            var ecarts = await _context.Ecarts
                .Include(e => e.Vol)
                .Where(e => e.Vol.FlightDate >= dateDebut && e.Vol.FlightDate <= dateFin)
                .Where(e => zone == null || e.Vol.Zone == zone)
                .ToListAsync();

            var statistics = new BudgetStatistics
            {
                NombreVols = vols.Count,
                MontantPrevu = vols.SelectMany(v => v.BonsCommandePrevisionnels).Sum(bcp => bcp.MontantTotal),
                MontantReel = vols.SelectMany(v => v.BonsLivraison)
                    .Where(bl => bl.Status == StatusBL.Valide)
                    .Sum(bl => bl.MontantTotal),
                NombreEcarts = ecarts.Count,
                MontantEcarts = ecarts.Sum(e => Math.Abs(e.EcartMontant))
            };

            statistics.EcartMontant = statistics.MontantReel - statistics.MontantPrevu;
            statistics.PourcentageEcart = statistics.MontantPrevu != 0 
                ? (statistics.EcartMontant / statistics.MontantPrevu) * 100 
                : 0;

            // Calculer les coûts par type d'article
            statistics.CoutsByType = vols.SelectMany(v => v.BonsLivraison)
                .Where(bl => bl.Status == StatusBL.Valide)
                .SelectMany(bl => bl.Lignes)
                .GroupBy(l => l.Article.Type)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.MontantLigne));

            // Calculer les coûts par zone
            statistics.CoutsByZone = vols.GroupBy(v => v.Zone)
                .ToDictionary(g => g.Key, g => g.SelectMany(v => v.BonsLivraison)
                    .Where(bl => bl.Status == StatusBL.Valide)
                    .Sum(bl => bl.MontantTotal));

            return statistics;
        }

        public async Task<DossierVol> GenerateCompleteDossierVolAsync(int volId)
        {
            _logger.LogInformation($"Génération du dossier de vol complet pour le vol {volId}");

            var vol = await _context.Vols
                .Include(v => v.BonsCommandePrevisionnels)
                    .ThenInclude(bcp => bcp.Lignes)
                .Include(v => v.BonsLivraison)
                    .ThenInclude(bl => bl.Lignes)
                .Include(v => v.VolBoitesMedicales)
                    .ThenInclude(vbm => vbm.BoiteMedicale)
                .FirstOrDefaultAsync(v => v.Id == volId);

            if (vol == null)
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé");

            // Vérifier s'il existe déjà un dossier
            var existingDossier = await _context.DossiersVol.FirstOrDefaultAsync(d => d.VolId == volId);
            if (existingDossier != null)
                throw new InvalidOperationException($"Un dossier existe déjà pour le vol {volId}");

            var ecarts = await _context.Ecarts.Where(e => e.VolId == volId).ToListAsync();
            var coutTotal = vol.BonsLivraison.Where(bl => bl.Status == StatusBL.Valide).Sum(bl => bl.MontantTotal);

            var numero = $"DV-{vol.FlightNumber}-{vol.FlightDate:yyyyMMdd}";
            var resume = GenerateVolSummary(vol, ecarts);

            var dossier = new DossierVol
            {
                VolId = volId,
                Numero = numero,
                Status = StatusDossierVol.Complete,
                DateCreation = DateTime.UtcNow,
                Resume = resume,
                Commentaires = $"Dossier généré automatiquement le {DateTime.Now:dd/MM/yyyy à HH:mm}",
                CoutTotal = coutTotal,
                NombreEcarts = ecarts.Count,
                MontantEcarts = ecarts.Sum(e => Math.Abs(e.EcartMontant)),
                CreatedAt = DateTime.UtcNow
            };

            _context.DossiersVol.Add(dossier);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Dossier de vol {dossier.Numero} créé pour le vol {volId}");
            return dossier;
        }

        public async Task<ValidationResult> ValidateVolConsistencyAsync(int volId)
        {
            var result = new ValidationResult { IsValid = true };

            var vol = await _context.Vols
                .Include(v => v.PlanHebergement)
                    .ThenInclude(ph => ph!.MenusPlanHebergement)
                        .ThenInclude(mph => mph.Menu)
                            .ThenInclude(m => m.MenuItems)
                .Include(v => v.BonsCommandePrevisionnels)
                    .ThenInclude(bcp => bcp.Lignes)
                .Include(v => v.BonsLivraison)
                    .ThenInclude(bl => bl.Lignes)
                .FirstOrDefaultAsync(v => v.Id == volId);

            if (vol == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Vol avec l'ID {volId} non trouvé");
                return result;
            }

            // Vérifier le plan d'hébergement
            if (vol.PlanHebergement == null)
            {
                result.Warnings.Add("Aucun plan d'hébergement défini");
                result.Recommendations.Add("Créer un plan d'hébergement pour ce vol");
            }

            // Vérifier les menus
            if (vol.PlanHebergement?.MenusPlanHebergement.Count == 0)
            {
                result.Warnings.Add("Aucun menu associé au plan d'hébergement");
                result.Recommendations.Add("Associer des menus au plan d'hébergement");
            }

            // Vérifier les BCP
            if (vol.BonsCommandePrevisionnels.Count == 0)
            {
                result.Warnings.Add("Aucun bon de commande prévisionnel créé");
                result.Recommendations.Add("Générer un BCP pour ce vol");
            }

            // Vérifier les BL
            var blValides = vol.BonsLivraison.Where(bl => bl.Status == StatusBL.Valide).ToList();
            if (blValides.Count == 0)
            {
                result.Warnings.Add("Aucun bon de livraison validé");
                result.Recommendations.Add("Valider les bons de livraison reçus");
            }

            // Vérifier les écarts non traités
            var ecartsNonTraites = await _context.Ecarts
                .Where(e => e.VolId == volId && e.Status == StatusEcart.EnAttente)
                .CountAsync();

            if (ecartsNonTraites > 0)
            {
                result.Warnings.Add($"{ecartsNonTraites} écart(s) en attente de traitement");
                result.Recommendations.Add("Traiter les écarts en attente");
            }

            return result;
        }

        public async Task<CostEstimate> CalculateVolCostEstimateAsync(int volId)
        {
            var vol = await _context.Vols
                .Include(v => v.PlanHebergement)
                    .ThenInclude(ph => ph!.PlanHebergementArticles)
                        .ThenInclude(pha => pha.Article)
                .Include(v => v.PlanHebergement)
                    .ThenInclude(ph => ph!.MenusPlanHebergement)
                        .ThenInclude(mph => mph.Menu)
                            .ThenInclude(m => m.MenuItems)
                                .ThenInclude(mi => mi.Article)
                .FirstOrDefaultAsync(v => v.Id == volId);

            if (vol == null)
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé");

            var estimate = new CostEstimate();
            var costBreakdown = new List<string>();

            if (vol.PlanHebergement != null)
            {
                // Calculer les coûts du plan d'hébergement
                foreach (var planArticle in vol.PlanHebergement.PlanHebergementArticles)
                {
                    var quantity = planArticle.QuantiteStandard * vol.EstimatedPassengers;
                    var cost = quantity * planArticle.Article.UnitPrice;
                    
                    estimate.CostsByType[planArticle.Article.Type] = 
                        estimate.CostsByType.GetValueOrDefault(planArticle.Article.Type, 0) + cost;
                    
                    costBreakdown.Add($"Plan: {planArticle.Article.Name} x {quantity} = {cost:C}");
                }

                // Calculer les coûts des menus
                foreach (var menuPlan in vol.PlanHebergement.MenusPlanHebergement)
                {
                    decimal menuCost = 0;
                    foreach (var menuItem in menuPlan.Menu.MenuItems)
                    {
                        var quantity = menuItem.Quantity * vol.EstimatedPassengers;
                        var cost = quantity * menuItem.Article.UnitPrice;
                        menuCost += cost;
                        
                        estimate.CostsByType[menuItem.Article.Type] = 
                            estimate.CostsByType.GetValueOrDefault(menuItem.Article.Type, 0) + cost;
                        
                        costBreakdown.Add($"Menu {menuPlan.Menu.Name}: {menuItem.Article.Name} x {quantity} = {cost:C}");
                    }
                    estimate.CostsByMenu[menuPlan.Menu.Name] = menuCost;
                }
            }

            estimate.EstimatedTotalCost = estimate.CostsByType.Values.Sum();
            estimate.CostPerPassenger = vol.EstimatedPassengers > 0 
                ? estimate.EstimatedTotalCost / vol.EstimatedPassengers 
                : 0;
            estimate.CostBreakdown = costBreakdown;

            return estimate;
        }

        public async Task<List<BoiteMedicale>> RecommendMedicalBoxesAsync(int volId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
                throw new ArgumentException($"Vol avec l'ID {volId} non trouvé");

            var recommendations = new List<BoiteMedicale>();

            // Règles de recommandation basées sur la durée et la destination
            var availableBoxes = await _context.BoitesMedicales
                .Where(b => b.IsActive && b.Status == StatusBoiteMedicale.Disponible && b.DateExpiration > DateTime.UtcNow)
                .ToListAsync();

            // Toujours recommander une boîte docteur
            var doctorBox = availableBoxes.FirstOrDefault(b => b.Type == TypeBoiteMedicale.BoiteDoctor);
            if (doctorBox != null)
                recommendations.Add(doctorBox);

            // Pour les vols longs (>4h), recommander une boîte pharmacie
            if (vol.Duration.TotalHours > 4)
            {
                var pharmacyBox = availableBoxes.FirstOrDefault(b => b.Type == TypeBoiteMedicale.BoitePharmacie);
                if (pharmacyBox != null)
                    recommendations.Add(pharmacyBox);
            }

            // Pour les destinations internationales, recommander kit premier secours
            if (vol.Zone.Contains("International") || vol.Destination.Length > 10)
            {
                var firstAidBox = availableBoxes.FirstOrDefault(b => b.Type == TypeBoiteMedicale.KitPremierSecours);
                if (firstAidBox != null)
                    recommendations.Add(firstAidBox);
            }

            return recommendations;
        }

        #region Private Helper Methods

        private async Task AddStandardArticlesToPlan(PlanHebergement plan, Vol vol)
        {
            // Ajouter des articles standard selon les règles métier
            var standardArticles = await _context.Articles
                .Where(a => a.IsActive && (
                    a.Type == TypeArticle.MaterielDivers || 
                    (a.Type == TypeArticle.Consommable && a.Name.Contains("Standard"))
                ))
                .ToListAsync();

            foreach (var article in standardArticles)
            {
                var quantiteStandard = CalculateStandardQuantity(article, vol, plan);
                if (quantiteStandard > 0)
                {
                    var planArticle = new PlanHebergementArticle
                    {
                        PlanHebergementId = plan.Id,
                        ArticleId = article.Id,
                        QuantiteStandard = quantiteStandard,
                        TypePassager = "All"
                    };

                    _context.PlanHebergementArticles.Add(planArticle);
                }
            }

            await _context.SaveChangesAsync();
        }

        private int CalculateStandardQuantity(Article article, Vol vol, PlanHebergement plan)
        {
            // Logique de calcul des quantités standard
            return article.Type switch
            {
                TypeArticle.MaterielDivers when article.Name.Contains("Couverture") => vol.EstimatedPassengers / 2,
                TypeArticle.MaterielDivers when article.Name.Contains("Oreiller") => vol.EstimatedPassengers / 3,
                TypeArticle.Consommable when article.Name.Contains("Serviette") => vol.EstimatedPassengers,
                _ => 0
            };
        }

        private async Task<Dictionary<int, int>> CalculateArticleQuantities(Vol vol)
        {
            var quantities = new Dictionary<int, int>();

            if (vol.PlanHebergement != null)
            {
                // Articles du plan d'hébergement
                foreach (var planArticle in vol.PlanHebergement.PlanHebergementArticles)
                {
                    var quantity = planArticle.QuantiteStandard * vol.EstimatedPassengers;
                    quantities[planArticle.ArticleId] = quantities.GetValueOrDefault(planArticle.ArticleId, 0) + quantity;
                }

                // Articles des menus
                foreach (var menuPlan in vol.PlanHebergement.MenusPlanHebergement)
                {
                    foreach (var menuItem in menuPlan.Menu.MenuItems)
                    {
                        var quantity = menuItem.Quantity * vol.EstimatedPassengers;
                        quantities[menuItem.ArticleId] = quantities.GetValueOrDefault(menuItem.ArticleId, 0) + quantity;
                    }
                }
            }

            return quantities;
        }

        private async Task<string> GenerateUniqueBcpNumber(Vol vol)
        {
            var baseNumber = $"BCP-{vol.FlightNumber}-{vol.FlightDate:yyyyMMdd}";
            var counter = 1;
            var numero = baseNumber;

            while (await _context.BonsCommandePrevisionnels.AnyAsync(b => b.Numero == numero))
            {
                numero = $"{baseNumber}-{counter:D2}";
                counter++;
            }

            return numero;
        }

        private Ecart? AnalyzeArticleEcart(BonLivraison bonLivraison, BonCommandePrevisionnelLigne bcpLigne, BonLivraisonLigne blLigne)
        {
            var ecartQuantite = blLigne.QuantiteLivree - bcpLigne.QuantiteCommandee;
            var ecartMontant = blLigne.MontantLigne - bcpLigne.MontantLigne;

            if (ecartQuantite == 0 && Math.Abs(ecartMontant) < 0.01m)
                return null;

            var typeEcart = ecartQuantite > 0 ? TypeEcart.QuantiteSuperieure :
                           ecartQuantite < 0 ? TypeEcart.QuantiteInferieure :
                           TypeEcart.PrixDifferent;

            return new Ecart
            {
                VolId = bonLivraison.VolId ?? bonLivraison.BonCommandePrevisionnel!.VolId,
                ArticleId = bcpLigne.ArticleId,
                BonCommandePrevisionnelId = bonLivraison.BonCommandePrevisionnelId,
                BonLivraisonId = bonLivraison.Id,
                TypeEcart = typeEcart,
                Status = StatusEcart.EnAttente,
                QuantiteCommandee = bcpLigne.QuantiteCommandee,
                QuantiteLivree = blLigne.QuantiteLivree,
                EcartQuantite = ecartQuantite,
                PrixCommande = bcpLigne.PrixUnitaire,
                PrixLivraison = blLigne.PrixUnitaire,
                EcartMontant = ecartMontant,
                Description = $"Écart détecté lors de la validation du BL {bonLivraison.Numero}",
                DateDetection = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }

        private Ecart CreateMissingArticleEcart(BonLivraison bonLivraison, BonCommandePrevisionnelLigne bcpLigne)
        {
            return new Ecart
            {
                VolId = bonLivraison.VolId ?? bonLivraison.BonCommandePrevisionnel!.VolId,
                ArticleId = bcpLigne.ArticleId,
                BonCommandePrevisionnelId = bonLivraison.BonCommandePrevisionnelId,
                BonLivraisonId = bonLivraison.Id,
                TypeEcart = TypeEcart.ArticleManquant,
                Status = StatusEcart.EnAttente,
                QuantiteCommandee = bcpLigne.QuantiteCommandee,
                QuantiteLivree = 0,
                EcartQuantite = -bcpLigne.QuantiteCommandee,
                PrixCommande = bcpLigne.PrixUnitaire,
                PrixLivraison = 0,
                EcartMontant = -bcpLigne.MontantLigne,
                Description = $"Article commandé mais non livré - BL {bonLivraison.Numero}",
                DateDetection = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }

        private Ecart CreateExtraArticleEcart(BonLivraison bonLivraison, BonLivraisonLigne blLigne)
        {
            return new Ecart
            {
                VolId = bonLivraison.VolId ?? bonLivraison.BonCommandePrevisionnel!.VolId,
                ArticleId = blLigne.ArticleId!.Value, // Guaranteed to have value by caller check
                BonCommandePrevisionnelId = bonLivraison.BonCommandePrevisionnelId,
                BonLivraisonId = bonLivraison.Id,
                TypeEcart = TypeEcart.ArticleEnPlus,
                Status = StatusEcart.EnAttente,
                QuantiteCommandee = 0,
                QuantiteLivree = blLigne.QuantiteLivree,
                EcartQuantite = blLigne.QuantiteLivree,
                PrixCommande = 0,
                PrixLivraison = blLigne.PrixUnitaire,
                EcartMontant = blLigne.MontantLigne,
                Description = $"Article livré mais non commandé - BL {bonLivraison.Numero}",
                DateDetection = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }

        private string GenerateVolSummary(Vol vol, List<Ecart> ecarts)
        {
            var summary = $"DOSSIER DE VOL - {vol.FlightNumber}\n";
            summary += $"Date: {vol.FlightDate:dd/MM/yyyy}\n";
            summary += $"Route: {vol.Origin} → {vol.Destination}\n";
            summary += $"Avion: {vol.Aircraft}\n";
            summary += $"Passagers prévus: {vol.EstimatedPassengers}, Réels: {vol.ActualPassengers}\n\n";

            summary += "COMMANDES ET LIVRAISONS:\n";
            summary += $"- Nombre de BCP: {vol.BonsCommandePrevisionnels.Count}\n";
            summary += $"- Nombre de BL: {vol.BonsLivraison.Count}\n";
            summary += $"- BL validés: {vol.BonsLivraison.Count(bl => bl.Status == StatusBL.Valide)}\n\n";

            if (ecarts.Any())
            {
                summary += "ÉCARTS DÉTECTÉS:\n";
                summary += $"- Nombre total: {ecarts.Count}\n";
                summary += $"- En attente: {ecarts.Count(e => e.Status == StatusEcart.EnAttente)}\n";
                summary += $"- Résolus: {ecarts.Count(e => e.Status == StatusEcart.Resolu)}\n";
                summary += $"- Montant total des écarts: {ecarts.Sum(e => Math.Abs(e.EcartMontant)):C}\n\n";
            }

            summary += "ÉQUIPEMENTS MÉDICAUX:\n";
            summary += $"- Boîtes médicales assignées: {vol.VolBoitesMedicales.Count}\n";

            return summary;
        }

        #endregion
    }
}
