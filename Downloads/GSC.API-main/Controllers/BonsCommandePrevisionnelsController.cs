using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;
using GsC.API.Services;
using System.Security.Claims;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BonsCommandePrevisionnelsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMenuService _menuService;

        public BonsCommandePrevisionnelsController(ApplicationDbContext context, IMenuService menuService)
        {
            _context = context;
            _menuService = menuService;
        }

        /// <summary>
        /// Récupère tous les bons de commande prévisionnels avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BonCommandePrevisionnelDto>>> GetBonsCommandePrevisionnels([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var bons = await _context.BonsCommandePrevisionnels
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BonCommandePrevisionnelDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    VolId = b.VolId,
                    DateCommande = b.DateCommande,
                    Status = b.Status,
                    Fournisseur = b.Fournisseur,
                    MontantTotal = b.MontantTotal,
                    Commentaires = b.Commentaires,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    CreatedBy = b.CreatedBy
                })
                .ToListAsync();

            return Ok(bons);
        }

        /// <summary>
        /// Récupère un BCP par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BonCommandePrevisionnelDetailsDto>> GetBonCommandePrevisionnel(int id)
        {
            var bon = await _context.BonsCommandePrevisionnels
                .Include(b => b.Vol)
                .Include(b => b.Lignes)
                    .ThenInclude(l => l.Article)
                .Include(b => b.Ecarts)
                    .ThenInclude(e => e.Article)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bon == null)
            {
                return NotFound($"Bon de commande prévisionnel avec l'ID {id} non trouvé.");
            }

            var bonDto = new BonCommandePrevisionnelDetailsDto
            {
                Id = bon.Id,
                Numero = bon.Numero,
                VolId = bon.VolId,
                DateCommande = bon.DateCommande,
                Status = bon.Status,
                Fournisseur = bon.Fournisseur,
                MontantTotal = bon.MontantTotal,
                Commentaires = bon.Commentaires,
                CreatedAt = bon.CreatedAt,
                UpdatedAt = bon.UpdatedAt,
                CreatedBy = bon.CreatedBy,
                Vol = new VolDto
                {
                    Id = bon.Vol.Id,
                    FlightNumber = bon.Vol.FlightNumber,
                    FlightDate = bon.Vol.FlightDate,
                    DepartureTime = bon.Vol.DepartureTime,
                    ArrivalTime = bon.Vol.ArrivalTime,
                    Aircraft = bon.Vol.Aircraft,
                    Origin = bon.Vol.Origin,
                    Destination = bon.Vol.Destination,
                    Zone = bon.Vol.Zone,
                    EstimatedPassengers = bon.Vol.EstimatedPassengers,
                    ActualPassengers = bon.Vol.ActualPassengers,
                    Duration = bon.Vol.Duration,
                    Season = bon.Vol.Season,
                    CreatedAt = bon.Vol.CreatedAt,
                    UpdatedAt = bon.Vol.UpdatedAt
                },
                Lignes = bon.Lignes.Select(l => new BonCommandePrevisionnelLigneDto
                {
                    Id = l.Id,
                    BonCommandePrevisionnelId = l.BonCommandePrevisionnelId,
                    ArticleId = l.ArticleId,
                    QuantiteCommandee = l.QuantiteCommandee,
                    PrixUnitaire = l.PrixUnitaire,
                    MontantLigne = l.MontantLigne,
                    Commentaires = l.Commentaires,
                    Article = new ArticleDto
                    {
                        Id = l.Article.Id,
                        Code = l.Article.Code,
                        Name = l.Article.Name,
                        Description = l.Article.Description,
                        Type = l.Article.Type,
                        Unit = l.Article.Unit,
                        UnitPrice = l.Article.UnitPrice,
                        Supplier = l.Article.Supplier,
                        IsActive = l.Article.IsActive,
                        CreatedAt = l.Article.CreatedAt,
                        UpdatedAt = l.Article.UpdatedAt
                    }
                }).ToList(),
                Ecarts = bon.Ecarts.Select(e => new EcartDto
                {
                    Id = e.Id,
                    VolId = e.VolId,
                    ArticleId = e.ArticleId,
                    BonCommandePrevisionnelId = e.BonCommandePrevisionnelId,
                    BonLivraisonId = e.BonLivraisonId,
                    TypeEcart = e.TypeEcart,
                    Status = e.Status,
                    QuantiteCommandee = e.QuantiteCommandee,
                    QuantiteLivree = e.QuantiteLivree,
                    EcartQuantite = e.EcartQuantite,
                    PrixCommande = e.PrixCommande,
                    PrixLivraison = e.PrixLivraison,
                    EcartMontant = e.EcartMontant,
                    Description = e.Description,
                    ActionCorrective = e.ActionCorrective,
                    ResponsableTraitement = e.ResponsableTraitement,
                    DateDetection = e.DateDetection,
                    DateResolution = e.DateResolution,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList()
            };

            return Ok(bonDto);
        }

        /// <summary>
        /// Crée un nouveau BCP avec ses lignes
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BonCommandePrevisionnelDto>> CreateBonCommandePrevisionnel(CreateBonCommandePrevisionnelDto createBonDto)
        {
            // Vérifier que le vol existe
            var vol = await _context.Vols.FindAsync(createBonDto.VolId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {createBonDto.VolId} non trouvé.");
            }

            // Vérifier l'unicité du numéro
            if (await _context.BonsCommandePrevisionnels.AnyAsync(b => b.Numero == createBonDto.Numero))
            {
                return BadRequest($"Un BCP avec le numéro '{createBonDto.Numero}' existe déjà.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var bon = new BonCommandePrevisionnel
                {
                    Numero = createBonDto.Numero,
                    VolId = createBonDto.VolId,
                    DateCommande = createBonDto.DateCommande,
                    Status = StatusBCP.Brouillon,
                    Fournisseur = createBonDto.Fournisseur,
                    MontantTotal = 0,
                    Commentaires = createBonDto.Commentaires,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = HttpContext.User?.Identity?.Name
                };

                _context.BonsCommandePrevisionnels.Add(bon);
                await _context.SaveChangesAsync();

                decimal montantTotal = 0;

                // Ajouter les lignes du BCP
                foreach (var ligneDto in createBonDto.Lignes)
                {
                    var article = await _context.Articles.FindAsync(ligneDto.ArticleId);
                    if (article == null)
                    {
                        throw new ArgumentException($"Article avec l'ID {ligneDto.ArticleId} non trouvé.");
                    }

                    var prixUnitaire = ligneDto.PrixUnitaire > 0 ? ligneDto.PrixUnitaire : article.UnitPrice;
                    var montantLigne = ligneDto.QuantiteCommandee * prixUnitaire;

                    var ligne = new BonCommandePrevisionnelLigne
                    {
                        BonCommandePrevisionnelId = bon.Id,
                        ArticleId = ligneDto.ArticleId,
                        QuantiteCommandee = ligneDto.QuantiteCommandee,
                        PrixUnitaire = prixUnitaire,
                        MontantLigne = montantLigne,
                        Commentaires = ligneDto.Commentaires
                    };

                    _context.BonCommandePrevisionnelLignes.Add(ligne);
                    montantTotal += montantLigne;
                }

                bon.MontantTotal = montantTotal;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var bonDto = new BonCommandePrevisionnelDto
                {
                    Id = bon.Id,
                    Numero = bon.Numero,
                    VolId = bon.VolId,
                    DateCommande = bon.DateCommande,
                    Status = bon.Status,
                    Fournisseur = bon.Fournisseur,
                    MontantTotal = bon.MontantTotal,
                    Commentaires = bon.Commentaires,
                    CreatedAt = bon.CreatedAt,
                    UpdatedAt = bon.UpdatedAt,
                    CreatedBy = bon.CreatedBy
                };

                return CreatedAtAction(nameof(GetBonCommandePrevisionnel), new { id = bon.Id }, bonDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Met à jour un BCP existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBonCommandePrevisionnel(int id, UpdateBonCommandePrevisionnelDto updateBonDto)
        {
            var bon = await _context.BonsCommandePrevisionnels.FindAsync(id);
            if (bon == null)
            {
                return NotFound($"BCP avec l'ID {id} non trouvé.");
            }

            if (!string.IsNullOrEmpty(updateBonDto.Numero) && updateBonDto.Numero != bon.Numero)
            {
                if (await _context.BonsCommandePrevisionnels.AnyAsync(b => b.Numero == updateBonDto.Numero && b.Id != id))
                {
                    return BadRequest($"Un BCP avec le numéro '{updateBonDto.Numero}' existe déjà.");
                }
                bon.Numero = updateBonDto.Numero;
            }

            if (updateBonDto.DateCommande.HasValue)
                bon.DateCommande = updateBonDto.DateCommande.Value;
            if (updateBonDto.Status.HasValue)
                bon.Status = updateBonDto.Status.Value;
            if (updateBonDto.Fournisseur != null)
                bon.Fournisseur = updateBonDto.Fournisseur;
            if (updateBonDto.Commentaires != null)
                bon.Commentaires = updateBonDto.Commentaires;

            bon.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BonCommandePrevisionnelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Supprime un BCP
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBonCommandePrevisionnel(int id)
        {
            var bon = await _context.BonsCommandePrevisionnels
                .Include(b => b.Lignes)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bon == null)
            {
                return NotFound($"BCP avec l'ID {id} non trouvé.");
            }

            if (bon.Status != StatusBCP.Brouillon)
            {
                return BadRequest("Seuls les BCP en statut brouillon peuvent être supprimés.");
            }

            _context.BonsCommandePrevisionnels.Remove(bon);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Génère automatiquement un BCP à partir du plan d'hébergement et des menus d'un vol
        /// </summary>
        [HttpPost("generate-from-vol/{volId}")]
        public async Task<ActionResult<BonCommandePrevisionnelDto>> GenerateBcpFromVol(int volId, [FromBody] string? fournisseur = null)
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
            {
                return NotFound($"Vol avec l'ID {volId} non trouvé.");
            }

            if (vol.PlanHebergement == null)
            {
                return BadRequest($"Aucun plan d'hébergement trouvé pour le vol {volId}.");
            }

            // Générer un numéro unique pour le BCP
            var numero = $"BCP-{vol.FlightNumber}-{vol.FlightDate:yyyyMMdd}-{DateTime.Now:HHmmss}";

            var createBcpDto = new CreateBonCommandePrevisionnelDto
            {
                Numero = numero,
                VolId = volId,
                DateCommande = DateTime.UtcNow,
                Fournisseur = fournisseur,
                Commentaires = $"BCP généré automatiquement pour le vol {vol.FlightNumber}",
                Lignes = new List<CreateBonCommandePrevisionnelLigneDto>()
            };

            // Dictionnaire pour éviter les doublons d'articles
            var articlesDict = new Dictionary<int, CreateBonCommandePrevisionnelLigneDto>();

            // Ajouter les articles du plan d'hébergement
            foreach (var planArticle in vol.PlanHebergement.PlanHebergementArticles)
            {
                var quantite = planArticle.QuantiteStandard * vol.EstimatedPassengers;
                if (articlesDict.ContainsKey(planArticle.ArticleId))
                {
                    articlesDict[planArticle.ArticleId].QuantiteCommandee += quantite;
                }
                else
                {
                    articlesDict[planArticle.ArticleId] = new CreateBonCommandePrevisionnelLigneDto
                    {
                        ArticleId = planArticle.ArticleId,
                        QuantiteCommandee = quantite,
                        PrixUnitaire = planArticle.Article.UnitPrice,
                        Commentaires = $"Plan d'hébergement - {planArticle.TypePassager}"
                    };
                }
            }

            // Ajouter les articles des menus
            foreach (var menuPlan in vol.PlanHebergement.MenusPlanHebergement)
            {
                foreach (var menuItem in menuPlan.Menu.MenuItems)
                {
                    var quantite = menuItem.Quantity * vol.EstimatedPassengers;
                    if (articlesDict.ContainsKey(menuItem.ArticleId))
                    {
                        articlesDict[menuItem.ArticleId].QuantiteCommandee += quantite;
                    }
                    else
                    {
                        articlesDict[menuItem.ArticleId] = new CreateBonCommandePrevisionnelLigneDto
                        {
                            ArticleId = menuItem.ArticleId,
                            QuantiteCommandee = quantite,
                            PrixUnitaire = menuItem.Article.UnitPrice,
                            Commentaires = $"Menu {menuPlan.Menu.Name} - {menuItem.TypePassager}"
                        };
                    }
                }
            }

            createBcpDto.Lignes = articlesDict.Values.ToList();

            return await CreateBonCommandePrevisionnel(createBcpDto);
        }

        /// <summary>
        /// Change le statut d'un BCP
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBcpStatus(int id, [FromBody] StatusBCP newStatus)
        {
            var bon = await _context.BonsCommandePrevisionnels.FindAsync(id);
            if (bon == null)
            {
                return NotFound($"BCP avec l'ID {id} non trouvé.");
            }

            bon.Status = newStatus;
            bon.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Recherche des BCP par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BonCommandePrevisionnelDto>>> SearchBcp(
            [FromQuery] string? numero,
            [FromQuery] int? volId,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] StatusBCP? status,
            [FromQuery] string? fournisseur,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.BonsCommandePrevisionnels.AsQueryable();

            if (!string.IsNullOrEmpty(numero))
                query = query.Where(b => b.Numero.Contains(numero));

            if (volId.HasValue)
                query = query.Where(b => b.VolId == volId.Value);

            if (dateDebut.HasValue)
                query = query.Where(b => b.DateCommande >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(b => b.DateCommande <= dateFin.Value);

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            if (!string.IsNullOrEmpty(fournisseur))
                query = query.Where(b => b.Fournisseur != null && b.Fournisseur.Contains(fournisseur));

            var bons = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BonCommandePrevisionnelDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    VolId = b.VolId,
                    DateCommande = b.DateCommande,
                    Status = b.Status,
                    Fournisseur = b.Fournisseur,
                    MontantTotal = b.MontantTotal,
                    Commentaires = b.Commentaires,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    CreatedBy = b.CreatedBy
                })
                .ToListAsync();

            return Ok(bons);
        }

        /// <summary>
        /// Génère automatiquement un BCP basé sur les menus assignés à un vol (Admin uniquement)
        /// </summary>
        [HttpPost("generate-from-menus/{volId}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<BonCommandePrevisionnelDto>> GenerateBcpFromMenus(int volId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Vérifier si le vol existe
                var vol = await _context.Vols.FindAsync(volId);
                if (vol == null)
                {
                    return NotFound($"Vol avec l'ID {volId} non trouvé.");
                }

                // Vérifier s'il y a déjà un BCP pour ce vol
                var existingBcp = await _context.BonsCommandePrevisionnels
                    .FirstOrDefaultAsync(bcp => bcp.VolId == volId);

                if (existingBcp != null)
                {
                    return BadRequest($"Un BCP existe déjà pour ce vol (Numéro: {existingBcp.Numero}). Supprimez-le d'abord si vous voulez en générer un nouveau.");
                }

                // Générer le BCP depuis les menus
                var bcpDto = await _menuService.GenerateBcpFromMenusAsync(volId, currentUserId);

                return CreatedAtAction(nameof(GetBonCommandePrevisionnel), new { id = bcpDto.Id }, bcpDto);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la génération du BCP: {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère les statistiques des menus pour un vol (Admin uniquement)
        /// </summary>
        [HttpGet("menu-statistics/{volId}")]
        [Authorize(Roles = "Administrator,Manager,User")]
        public async Task<ActionResult<MenuStatisticsDto>> GetMenuStatistics(int volId)
        {
            try
            {
                var statistics = await _menuService.GetMenuStatisticsAsync(volId);
                return Ok(statistics);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors du calcul des statistiques: {ex.Message}");
            }
        }

        private bool BonCommandePrevisionnelExists(int id)
        {
            return _context.BonsCommandePrevisionnels.Any(e => e.Id == id);
        }
    }
}
