using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlansHebergementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlansHebergementController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les plans d'hébergement avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanHebergementDto>>> GetPlansHebergement([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var plans = await _context.PlansHebergement
                .Where(p => p.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PlanHebergementDto
                {
                    Id = p.Id,
                    VolId = p.VolId,
                    Name = p.Name,
                    Description = p.Description,
                    Season = p.Season,
                    AircraftType = p.AircraftType,
                    Zone = p.Zone,
                    FlightDuration = p.FlightDuration,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(plans);
        }

        /// <summary>
        /// Récupère un plan d'hébergement par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PlanHebergementDetailsDto>> GetPlanHebergement(int id)
        {
            var plan = await _context.PlansHebergement
                .Include(p => p.Vol)
                .Include(p => p.MenusPlanHebergement)
                    .ThenInclude(mph => mph.Menu)
                .Include(p => p.PlanHebergementArticles)
                    .ThenInclude(pha => pha.Article)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
            {
                return NotFound($"Plan d'hébergement avec l'ID {id} non trouvé.");
            }

            var planDto = new PlanHebergementDetailsDto
            {
                Id = plan.Id,
                VolId = plan.VolId,
                Name = plan.Name,
                Description = plan.Description,
                Season = plan.Season,
                AircraftType = plan.AircraftType,
                Zone = plan.Zone,
                FlightDuration = plan.FlightDuration,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                Vol = new VolDto
                {
                    Id = plan.Vol.Id,
                    FlightNumber = plan.Vol.FlightNumber,
                    FlightDate = plan.Vol.FlightDate,
                    DepartureTime = plan.Vol.DepartureTime,
                    ArrivalTime = plan.Vol.ArrivalTime,
                    Aircraft = plan.Vol.Aircraft,
                    Origin = plan.Vol.Origin,
                    Destination = plan.Vol.Destination,
                    Zone = plan.Vol.Zone,
                    EstimatedPassengers = plan.Vol.EstimatedPassengers,
                    ActualPassengers = plan.Vol.ActualPassengers,
                    Duration = plan.Vol.Duration,
                    Season = plan.Vol.Season,
                    CreatedAt = plan.Vol.CreatedAt,
                    UpdatedAt = plan.Vol.UpdatedAt
                },
                Menus = plan.MenusPlanHebergement.Select(mph => new MenuDto
                {
                    Id = mph.Menu.Id,
                    Name = mph.Menu.Name,
                    Description = mph.Menu.Description,
                    TypePassager = mph.Menu.TypePassager,
                    Season = mph.Menu.Season,
                    Zone = mph.Menu.Zone,
                    IsActive = mph.Menu.IsActive,
                    CreatedAt = mph.Menu.CreatedAt,
                    UpdatedAt = mph.Menu.UpdatedAt
                }).ToList(),
                Articles = plan.PlanHebergementArticles.Select(pha => new PlanHebergementArticleDto
                {
                    Id = pha.Id,
                    PlanHebergementId = pha.PlanHebergementId,
                    ArticleId = pha.ArticleId,
                    QuantiteStandard = pha.QuantiteStandard,
                    TypePassager = pha.TypePassager,
                    Article = new ArticleDto
                    {
                        Id = pha.Article.Id,
                        Code = pha.Article.Code,
                        Name = pha.Article.Name,
                        Description = pha.Article.Description,
                        Type = pha.Article.Type,
                        Unit = pha.Article.Unit,
                        UnitPrice = pha.Article.UnitPrice,
                        Supplier = pha.Article.Supplier,
                        IsActive = pha.Article.IsActive,
                        CreatedAt = pha.Article.CreatedAt,
                        UpdatedAt = pha.Article.UpdatedAt
                    }
                }).ToList()
            };

            return Ok(planDto);
        }

        /// <summary>
        /// Crée un nouveau plan d'hébergement avec ses articles
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PlanHebergementDto>> CreatePlanHebergement(CreatePlanHebergementDto createPlanDto)
        {
            // Vérifier que le vol existe
            var vol = await _context.Vols.FindAsync(createPlanDto.VolId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {createPlanDto.VolId} non trouvé.");
            }

            // Vérifier qu'il n'y a pas déjà un plan pour ce vol
            if (await _context.PlansHebergement.AnyAsync(p => p.VolId == createPlanDto.VolId))
            {
                return BadRequest($"Un plan d'hébergement existe déjà pour le vol {createPlanDto.VolId}.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var plan = new PlanHebergement
                {
                    VolId = createPlanDto.VolId,
                    Name = createPlanDto.Name,
                    Description = createPlanDto.Description,
                    Season = createPlanDto.Season,
                    AircraftType = createPlanDto.AircraftType,
                    Zone = createPlanDto.Zone,
                    FlightDuration = createPlanDto.FlightDuration,
                    IsActive = createPlanDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PlansHebergement.Add(plan);
                await _context.SaveChangesAsync();

                // Ajouter les articles du plan
                foreach (var articleDto in createPlanDto.Articles)
                {
                    var planArticle = new PlanHebergementArticle
                    {
                        PlanHebergementId = plan.Id,
                        ArticleId = articleDto.ArticleId,
                        QuantiteStandard = articleDto.QuantiteStandard,
                        TypePassager = articleDto.TypePassager
                    };

                    _context.PlanHebergementArticles.Add(planArticle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var planDto = new PlanHebergementDto
                {
                    Id = plan.Id,
                    VolId = plan.VolId,
                    Name = plan.Name,
                    Description = plan.Description,
                    Season = plan.Season,
                    AircraftType = plan.AircraftType,
                    Zone = plan.Zone,
                    FlightDuration = plan.FlightDuration,
                    IsActive = plan.IsActive,
                    CreatedAt = plan.CreatedAt,
                    UpdatedAt = plan.UpdatedAt
                };

                return CreatedAtAction(nameof(GetPlanHebergement), new { id = plan.Id }, planDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Met à jour un plan d'hébergement existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlanHebergement(int id, UpdatePlanHebergementDto updatePlanDto)
        {
            var plan = await _context.PlansHebergement.FindAsync(id);
            if (plan == null)
            {
                return NotFound($"Plan d'hébergement avec l'ID {id} non trouvé.");
            }

            if (!string.IsNullOrEmpty(updatePlanDto.Name))
                plan.Name = updatePlanDto.Name;
            if (updatePlanDto.Description != null)
                plan.Description = updatePlanDto.Description;
            if (!string.IsNullOrEmpty(updatePlanDto.Season))
                plan.Season = updatePlanDto.Season;
            if (!string.IsNullOrEmpty(updatePlanDto.AircraftType))
                plan.AircraftType = updatePlanDto.AircraftType;
            if (!string.IsNullOrEmpty(updatePlanDto.Zone))
                plan.Zone = updatePlanDto.Zone;
            if (updatePlanDto.FlightDuration.HasValue)
                plan.FlightDuration = updatePlanDto.FlightDuration.Value;
            if (updatePlanDto.IsActive.HasValue)
                plan.IsActive = updatePlanDto.IsActive.Value;

            plan.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlanHebergementExists(id))
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
        /// Supprime un plan d'hébergement (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlanHebergement(int id)
        {
            var plan = await _context.PlansHebergement.FindAsync(id);
            if (plan == null)
            {
                return NotFound($"Plan d'hébergement avec l'ID {id} non trouvé.");
            }

            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Ajoute un article à un plan d'hébergement
        /// </summary>
        [HttpPost("{planId}/articles")]
        public async Task<ActionResult<PlanHebergementArticleDto>> AddArticleToPlan(int planId, CreatePlanHebergementArticleDto createArticleDto)
        {
            var plan = await _context.PlansHebergement.FindAsync(planId);
            if (plan == null)
            {
                return NotFound($"Plan d'hébergement avec l'ID {planId} non trouvé.");
            }

            var article = await _context.Articles.FindAsync(createArticleDto.ArticleId);
            if (article == null)
            {
                return NotFound($"Article avec l'ID {createArticleDto.ArticleId} non trouvé.");
            }

            var planArticle = new PlanHebergementArticle
            {
                PlanHebergementId = planId,
                ArticleId = createArticleDto.ArticleId,
                QuantiteStandard = createArticleDto.QuantiteStandard,
                TypePassager = createArticleDto.TypePassager
            };

            _context.PlanHebergementArticles.Add(planArticle);
            await _context.SaveChangesAsync();

            var planArticleDto = new PlanHebergementArticleDto
            {
                Id = planArticle.Id,
                PlanHebergementId = planArticle.PlanHebergementId,
                ArticleId = planArticle.ArticleId,
                QuantiteStandard = planArticle.QuantiteStandard,
                TypePassager = planArticle.TypePassager,
                Article = new ArticleDto
                {
                    Id = article.Id,
                    Code = article.Code,
                    Name = article.Name,
                    Description = article.Description,
                    Type = article.Type,
                    Unit = article.Unit,
                    UnitPrice = article.UnitPrice,
                    Supplier = article.Supplier,
                    IsActive = article.IsActive,
                    CreatedAt = article.CreatedAt,
                    UpdatedAt = article.UpdatedAt
                }
            };

            return Ok(planArticleDto);
        }

        /// <summary>
        /// Met à jour un article d'un plan d'hébergement
        /// </summary>
        [HttpPut("articles/{articleId}")]
        public async Task<IActionResult> UpdatePlanArticle(int articleId, UpdatePlanHebergementArticleDto updateArticleDto)
        {
            var planArticle = await _context.PlanHebergementArticles.FindAsync(articleId);
            if (planArticle == null)
            {
                return NotFound($"Article de plan avec l'ID {articleId} non trouvé.");
            }

            if (updateArticleDto.QuantiteStandard.HasValue)
                planArticle.QuantiteStandard = updateArticleDto.QuantiteStandard.Value;
            if (updateArticleDto.TypePassager != null)
                planArticle.TypePassager = updateArticleDto.TypePassager;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlanHebergementArticleExists(articleId))
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
        /// Supprime un article d'un plan d'hébergement
        /// </summary>
        [HttpDelete("articles/{articleId}")]
        public async Task<IActionResult> DeletePlanArticle(int articleId)
        {
            var planArticle = await _context.PlanHebergementArticles.FindAsync(articleId);
            if (planArticle == null)
            {
                return NotFound($"Article de plan avec l'ID {articleId} non trouvé.");
            }

            _context.PlanHebergementArticles.Remove(planArticle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Associe un menu à un plan d'hébergement
        /// </summary>
        [HttpPost("{planId}/menus/{menuId}")]
        public async Task<IActionResult> AssociateMenuToPlan(int planId, int menuId)
        {
            var plan = await _context.PlansHebergement.FindAsync(planId);
            if (plan == null)
            {
                return NotFound($"Plan d'hébergement avec l'ID {planId} non trouvé.");
            }

            var menu = await _context.Menus.FindAsync(menuId);
            if (menu == null)
            {
                return NotFound($"Menu avec l'ID {menuId} non trouvé.");
            }

            // Vérifier si l'association existe déjà
            if (await _context.MenusPlanHebergement.AnyAsync(mph => mph.PlanHebergementId == planId && mph.MenuId == menuId))
            {
                return BadRequest("Cette association existe déjà.");
            }

            var menuPlan = new MenuPlanHebergement
            {
                MenuId = menuId,
                PlanHebergementId = planId
            };

            _context.MenusPlanHebergement.Add(menuPlan);
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Dissocie un menu d'un plan d'hébergement
        /// </summary>
        [HttpDelete("{planId}/menus/{menuId}")]
        public async Task<IActionResult> DissociateMenuFromPlan(int planId, int menuId)
        {
            var menuPlan = await _context.MenusPlanHebergement
                .FirstOrDefaultAsync(mph => mph.PlanHebergementId == planId && mph.MenuId == menuId);

            if (menuPlan == null)
            {
                return NotFound("Association non trouvée.");
            }

            _context.MenusPlanHebergement.Remove(menuPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Récupère le plan d'hébergement d'un vol
        /// </summary>
        [HttpGet("by-vol/{volId}")]
        public async Task<ActionResult<PlanHebergementDetailsDto>> GetPlanByVol(int volId)
        {
            var plan = await _context.PlansHebergement
                .Include(p => p.Vol)
                .Include(p => p.MenusPlanHebergement)
                    .ThenInclude(mph => mph.Menu)
                .Include(p => p.PlanHebergementArticles)
                    .ThenInclude(pha => pha.Article)
                .FirstOrDefaultAsync(p => p.VolId == volId);

            if (plan == null)
            {
                return NotFound($"Aucun plan d'hébergement trouvé pour le vol {volId}.");
            }

            return await GetPlanHebergement(plan.Id);
        }

        private bool PlanHebergementExists(int id)
        {
            return _context.PlansHebergement.Any(e => e.Id == id);
        }

        private bool PlanHebergementArticleExists(int id)
        {
            return _context.PlanHebergementArticles.Any(e => e.Id == id);
        }
    }
}
