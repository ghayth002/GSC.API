using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EcartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EcartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les écarts avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EcartDto>>> GetEcarts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var ecarts = await _context.Ecarts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EcartDto
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
                })
                .ToListAsync();

            return Ok(ecarts);
        }

        /// <summary>
        /// Récupère un écart par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EcartDetailsDto>> GetEcart(int id)
        {
            var ecart = await _context.Ecarts
                .Include(e => e.Vol)
                .Include(e => e.Article)
                .Include(e => e.BonCommandePrevisionnel)
                .Include(e => e.BonLivraison)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ecart == null)
            {
                return NotFound($"Écart avec l'ID {id} non trouvé.");
            }

            var ecartDto = new EcartDetailsDto
            {
                Id = ecart.Id,
                VolId = ecart.VolId,
                ArticleId = ecart.ArticleId,
                BonCommandePrevisionnelId = ecart.BonCommandePrevisionnelId,
                BonLivraisonId = ecart.BonLivraisonId,
                TypeEcart = ecart.TypeEcart,
                Status = ecart.Status,
                QuantiteCommandee = ecart.QuantiteCommandee,
                QuantiteLivree = ecart.QuantiteLivree,
                EcartQuantite = ecart.EcartQuantite,
                PrixCommande = ecart.PrixCommande,
                PrixLivraison = ecart.PrixLivraison,
                EcartMontant = ecart.EcartMontant,
                Description = ecart.Description,
                ActionCorrective = ecart.ActionCorrective,
                ResponsableTraitement = ecart.ResponsableTraitement,
                DateDetection = ecart.DateDetection,
                DateResolution = ecart.DateResolution,
                CreatedAt = ecart.CreatedAt,
                UpdatedAt = ecart.UpdatedAt,
                Vol = new VolDto
                {
                    Id = ecart.Vol.Id,
                    FlightNumber = ecart.Vol.FlightNumber,
                    FlightDate = ecart.Vol.FlightDate,
                    DepartureTime = ecart.Vol.DepartureTime,
                    ArrivalTime = ecart.Vol.ArrivalTime,
                    Aircraft = ecart.Vol.Aircraft,
                    Origin = ecart.Vol.Origin,
                    Destination = ecart.Vol.Destination,
                    Zone = ecart.Vol.Zone,
                    EstimatedPassengers = ecart.Vol.EstimatedPassengers,
                    ActualPassengers = ecart.Vol.ActualPassengers,
                    Duration = ecart.Vol.Duration,
                    Season = ecart.Vol.Season,
                    CreatedAt = ecart.Vol.CreatedAt,
                    UpdatedAt = ecart.Vol.UpdatedAt
                },
                Article = new ArticleDto
                {
                    Id = ecart.Article.Id,
                    Code = ecart.Article.Code,
                    Name = ecart.Article.Name,
                    Description = ecart.Article.Description,
                    Type = ecart.Article.Type,
                    Unit = ecart.Article.Unit,
                    UnitPrice = ecart.Article.UnitPrice,
                    Supplier = ecart.Article.Supplier,
                    IsActive = ecart.Article.IsActive,
                    CreatedAt = ecart.Article.CreatedAt,
                    UpdatedAt = ecart.Article.UpdatedAt
                },
                BonCommandePrevisionnel = ecart.BonCommandePrevisionnel != null ? new BonCommandePrevisionnelDto
                {
                    Id = ecart.BonCommandePrevisionnel.Id,
                    Numero = ecart.BonCommandePrevisionnel.Numero,
                    VolId = ecart.BonCommandePrevisionnel.VolId,
                    DateCommande = ecart.BonCommandePrevisionnel.DateCommande,
                    Status = ecart.BonCommandePrevisionnel.Status,
                    Fournisseur = ecart.BonCommandePrevisionnel.Fournisseur,
                    MontantTotal = ecart.BonCommandePrevisionnel.MontantTotal,
                    Commentaires = ecart.BonCommandePrevisionnel.Commentaires,
                    CreatedAt = ecart.BonCommandePrevisionnel.CreatedAt,
                    UpdatedAt = ecart.BonCommandePrevisionnel.UpdatedAt,
                    CreatedBy = ecart.BonCommandePrevisionnel.CreatedBy
                } : null,
                BonLivraison = ecart.BonLivraison != null ? new BonLivraisonDto
                {
                    Id = ecart.BonLivraison.Id,
                    Numero = ecart.BonLivraison.Numero,
                    VolId = ecart.BonLivraison.VolId,
                    BonCommandePrevisionnelId = ecart.BonLivraison.BonCommandePrevisionnelId,
                    DateLivraison = ecart.BonLivraison.DateLivraison,
                    Status = ecart.BonLivraison.Status,
                    Fournisseur = ecart.BonLivraison.Fournisseur,
                    Livreur = ecart.BonLivraison.Livreur,
                    Commentaires = ecart.BonLivraison.Commentaires,
                    MontantTotal = ecart.BonLivraison.MontantTotal,
                    CreatedAt = ecart.BonLivraison.CreatedAt,
                    UpdatedAt = ecart.BonLivraison.UpdatedAt,
                    ValidatedBy = ecart.BonLivraison.ValidatedBy,
                    ValidationDate = ecart.BonLivraison.ValidationDate
                } : null
            };

            return Ok(ecartDto);
        }

        /// <summary>
        /// Crée un nouvel écart manuellement
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EcartDto>> CreateEcart(CreateEcartDto createEcartDto)
        {
            // Vérifier que le vol existe
            var vol = await _context.Vols.FindAsync(createEcartDto.VolId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {createEcartDto.VolId} non trouvé.");
            }

            // Vérifier que l'article existe
            var article = await _context.Articles.FindAsync(createEcartDto.ArticleId);
            if (article == null)
            {
                return NotFound($"Article avec l'ID {createEcartDto.ArticleId} non trouvé.");
            }

            // Calculer les écarts
            var ecartQuantite = createEcartDto.QuantiteLivree - createEcartDto.QuantiteCommandee;
            var ecartMontant = (createEcartDto.QuantiteLivree * createEcartDto.PrixLivraison) - 
                              (createEcartDto.QuantiteCommandee * createEcartDto.PrixCommande);

            var ecart = new Ecart
            {
                VolId = createEcartDto.VolId,
                ArticleId = createEcartDto.ArticleId,
                BonCommandePrevisionnelId = createEcartDto.BonCommandePrevisionnelId,
                BonLivraisonId = createEcartDto.BonLivraisonId,
                TypeEcart = createEcartDto.TypeEcart,
                Status = StatusEcart.EnAttente,
                QuantiteCommandee = createEcartDto.QuantiteCommandee,
                QuantiteLivree = createEcartDto.QuantiteLivree,
                EcartQuantite = ecartQuantite,
                PrixCommande = createEcartDto.PrixCommande,
                PrixLivraison = createEcartDto.PrixLivraison,
                EcartMontant = ecartMontant,
                Description = createEcartDto.Description,
                ActionCorrective = createEcartDto.ActionCorrective,
                ResponsableTraitement = createEcartDto.ResponsableTraitement,
                DateDetection = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Ecarts.Add(ecart);
            await _context.SaveChangesAsync();

            var ecartDto = new EcartDto
            {
                Id = ecart.Id,
                VolId = ecart.VolId,
                ArticleId = ecart.ArticleId,
                BonCommandePrevisionnelId = ecart.BonCommandePrevisionnelId,
                BonLivraisonId = ecart.BonLivraisonId,
                TypeEcart = ecart.TypeEcart,
                Status = ecart.Status,
                QuantiteCommandee = ecart.QuantiteCommandee,
                QuantiteLivree = ecart.QuantiteLivree,
                EcartQuantite = ecart.EcartQuantite,
                PrixCommande = ecart.PrixCommande,
                PrixLivraison = ecart.PrixLivraison,
                EcartMontant = ecart.EcartMontant,
                Description = ecart.Description,
                ActionCorrective = ecart.ActionCorrective,
                ResponsableTraitement = ecart.ResponsableTraitement,
                DateDetection = ecart.DateDetection,
                DateResolution = ecart.DateResolution,
                CreatedAt = ecart.CreatedAt,
                UpdatedAt = ecart.UpdatedAt
            };

            return CreatedAtAction(nameof(GetEcart), new { id = ecart.Id }, ecartDto);
        }

        /// <summary>
        /// Met à jour un écart existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEcart(int id, UpdateEcartDto updateEcartDto)
        {
            var ecart = await _context.Ecarts.FindAsync(id);
            if (ecart == null)
            {
                return NotFound($"Écart avec l'ID {id} non trouvé.");
            }

            if (updateEcartDto.Status.HasValue)
            {
                ecart.Status = updateEcartDto.Status.Value;
                if (updateEcartDto.Status.Value == StatusEcart.Resolu && ecart.DateResolution == null)
                {
                    ecart.DateResolution = updateEcartDto.DateResolution ?? DateTime.UtcNow;
                }
            }

            if (updateEcartDto.QuantiteCommandee.HasValue)
            {
                ecart.QuantiteCommandee = updateEcartDto.QuantiteCommandee.Value;
                ecart.EcartQuantite = ecart.QuantiteLivree - ecart.QuantiteCommandee;
            }

            if (updateEcartDto.QuantiteLivree.HasValue)
            {
                ecart.QuantiteLivree = updateEcartDto.QuantiteLivree.Value;
                ecart.EcartQuantite = ecart.QuantiteLivree - ecart.QuantiteCommandee;
            }

            if (updateEcartDto.PrixCommande.HasValue)
            {
                ecart.PrixCommande = updateEcartDto.PrixCommande.Value;
            }

            if (updateEcartDto.PrixLivraison.HasValue)
            {
                ecart.PrixLivraison = updateEcartDto.PrixLivraison.Value;
            }

            // Recalculer l'écart montant
            ecart.EcartMontant = (ecart.QuantiteLivree * ecart.PrixLivraison) - (ecart.QuantiteCommandee * ecart.PrixCommande);

            if (updateEcartDto.Description != null)
                ecart.Description = updateEcartDto.Description;
            if (updateEcartDto.ActionCorrective != null)
                ecart.ActionCorrective = updateEcartDto.ActionCorrective;
            if (updateEcartDto.ResponsableTraitement != null)
                ecart.ResponsableTraitement = updateEcartDto.ResponsableTraitement;

            ecart.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EcartExists(id))
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
        /// Supprime un écart
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEcart(int id)
        {
            var ecart = await _context.Ecarts.FindAsync(id);
            if (ecart == null)
            {
                return NotFound($"Écart avec l'ID {id} non trouvé.");
            }

            if (ecart.Status == StatusEcart.Resolu)
            {
                return BadRequest("Un écart résolu ne peut pas être supprimé.");
            }

            _context.Ecarts.Remove(ecart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Résout un écart
        /// </summary>
        [HttpPost("{id}/resolve")]
        public async Task<IActionResult> ResolveEcart(int id, [FromBody] string actionCorrective)
        {
            var ecart = await _context.Ecarts.FindAsync(id);
            if (ecart == null)
            {
                return NotFound($"Écart avec l'ID {id} non trouvé.");
            }

            if (ecart.Status == StatusEcart.Resolu)
            {
                return BadRequest("Cet écart est déjà résolu.");
            }

            ecart.Status = StatusEcart.Resolu;
            ecart.ActionCorrective = actionCorrective;
            ecart.ResponsableTraitement = HttpContext.User?.Identity?.Name;
            ecart.DateResolution = DateTime.UtcNow;
            ecart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Écart résolu avec succès" });
        }

        /// <summary>
        /// Accepte un écart (pas de correction nécessaire)
        /// </summary>
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptEcart(int id, [FromBody] string? justification = null)
        {
            var ecart = await _context.Ecarts.FindAsync(id);
            if (ecart == null)
            {
                return NotFound($"Écart avec l'ID {id} non trouvé.");
            }

            if (ecart.Status == StatusEcart.Accepte)
            {
                return BadRequest("Cet écart est déjà accepté.");
            }

            ecart.Status = StatusEcart.Accepte;
            ecart.ActionCorrective = justification ?? "Écart accepté sans action corrective";
            ecart.ResponsableTraitement = HttpContext.User?.Identity?.Name;
            ecart.DateResolution = DateTime.UtcNow;
            ecart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Écart accepté avec succès" });
        }

        /// <summary>
        /// Rejette un écart
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectEcart(int id, [FromBody] string motifRejet)
        {
            var ecart = await _context.Ecarts.FindAsync(id);
            if (ecart == null)
            {
                return NotFound($"Écart avec l'ID {id} non trouvé.");
            }

            if (ecart.Status == StatusEcart.Rejete)
            {
                return BadRequest("Cet écart est déjà rejeté.");
            }

            ecart.Status = StatusEcart.Rejete;
            ecart.ActionCorrective = $"REJETÉ: {motifRejet}";
            ecart.ResponsableTraitement = HttpContext.User?.Identity?.Name;
            ecart.DateResolution = DateTime.UtcNow;
            ecart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Écart rejeté avec succès" });
        }

        /// <summary>
        /// Recherche des écarts par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EcartDto>>> SearchEcarts(
            [FromQuery] int? volId,
            [FromQuery] int? articleId,
            [FromQuery] int? bcpId,
            [FromQuery] int? blId,
            [FromQuery] TypeEcart? typeEcart,
            [FromQuery] StatusEcart? status,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? responsable,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Ecarts.AsQueryable();

            if (volId.HasValue)
                query = query.Where(e => e.VolId == volId.Value);

            if (articleId.HasValue)
                query = query.Where(e => e.ArticleId == articleId.Value);

            if (bcpId.HasValue)
                query = query.Where(e => e.BonCommandePrevisionnelId == bcpId.Value);

            if (blId.HasValue)
                query = query.Where(e => e.BonLivraisonId == blId.Value);

            if (typeEcart.HasValue)
                query = query.Where(e => e.TypeEcart == typeEcart.Value);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            if (dateDebut.HasValue)
                query = query.Where(e => e.DateDetection >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(e => e.DateDetection <= dateFin.Value);

            if (!string.IsNullOrEmpty(responsable))
                query = query.Where(e => e.ResponsableTraitement != null && e.ResponsableTraitement.Contains(responsable));

            var ecarts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EcartDto
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
                })
                .ToListAsync();

            return Ok(ecarts);
        }

        /// <summary>
        /// Récupère les statistiques des écarts
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetEcartsStatistics([FromQuery] DateTime? dateDebut, [FromQuery] DateTime? dateFin)
        {
            var query = _context.Ecarts.AsQueryable();

            if (dateDebut.HasValue)
                query = query.Where(e => e.DateDetection >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(e => e.DateDetection <= dateFin.Value);

            var statistics = await query
                .GroupBy(e => e.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    MontantTotal = g.Sum(e => Math.Abs(e.EcartMontant))
                })
                .ToListAsync();

            var typeStatistics = await query
                .GroupBy(e => e.TypeEcart)
                .Select(g => new
                {
                    TypeEcart = g.Key,
                    Count = g.Count(),
                    MontantTotal = g.Sum(e => Math.Abs(e.EcartMontant))
                })
                .ToListAsync();

            var totalEcarts = await query.CountAsync();
            var montantTotalEcarts = await query.SumAsync(e => Math.Abs(e.EcartMontant));

            return Ok(new
            {
                TotalEcarts = totalEcarts,
                MontantTotalEcarts = montantTotalEcarts,
                StatistiquesParStatus = statistics,
                StatistiquesParType = typeStatistics
            });
        }

        private bool EcartExists(int id)
        {
            return _context.Ecarts.Any(e => e.Id == id);
        }
    }
}
