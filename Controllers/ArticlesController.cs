using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les articles avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var articles = await _context.Articles
                .Where(a => a.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleDto
                {
                    Id = a.Id,
                    Code = a.Code,
                    Name = a.Name,
                    Description = a.Description,
                    Type = a.Type,
                    Unit = a.Unit,
                    UnitPrice = a.UnitPrice,
                    Supplier = a.Supplier,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(articles);
        }

        /// <summary>
        /// Récupère un article par ID avec détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDetailsDto>> GetArticle(int id)
        {
            var article = await _context.Articles
                .Include(a => a.MenuItems)
                    .ThenInclude(mi => mi.Menu)
                .Include(a => a.PlanHebergementArticles)
                    .ThenInclude(pha => pha.PlanHebergement)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound($"Article avec l'ID {id} non trouvé.");
            }

            var articleDto = new ArticleDetailsDto
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
                UpdatedAt = article.UpdatedAt,
                MenuItems = article.MenuItems.Select(mi => new MenuItemDto
                {
                    Id = mi.Id,
                    MenuId = mi.MenuId,
                    ArticleId = mi.ArticleId,
                    Quantity = mi.Quantity,
                    TypePassager = mi.TypePassager,
                    CreatedAt = mi.CreatedAt,
                    Article = new ArticleDto()
                }).ToList(),
                PlanHebergementArticles = article.PlanHebergementArticles.Select(pha => new PlanHebergementArticleDto
                {
                    Id = pha.Id,
                    PlanHebergementId = pha.PlanHebergementId,
                    ArticleId = pha.ArticleId,
                    QuantiteStandard = pha.QuantiteStandard,
                    TypePassager = pha.TypePassager,
                    Article = new ArticleDto()
                }).ToList()
            };

            return Ok(articleDto);
        }

        /// <summary>
        /// Crée un nouveau article
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ArticleDto>> CreateArticle(CreateArticleDto createArticleDto)
        {
            // Vérifier si le code existe déjà
            if (await _context.Articles.AnyAsync(a => a.Code == createArticleDto.Code))
            {
                return BadRequest($"Un article avec le code '{createArticleDto.Code}' existe déjà.");
            }

            var article = new Article
            {
                Code = createArticleDto.Code,
                Name = createArticleDto.Name,
                Description = createArticleDto.Description,
                Type = createArticleDto.Type,
                Unit = createArticleDto.Unit,
                UnitPrice = createArticleDto.UnitPrice,
                Supplier = createArticleDto.Supplier,
                IsActive = createArticleDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            var articleDto = new ArticleDto
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
            };

            return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, articleDto);
        }

        /// <summary>
        /// Met à jour un article existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, UpdateArticleDto updateArticleDto)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound($"Article avec l'ID {id} non trouvé.");
            }

            // Vérifier si le nouveau code existe déjà (si changé)
            if (!string.IsNullOrEmpty(updateArticleDto.Code) && updateArticleDto.Code != article.Code)
            {
                if (await _context.Articles.AnyAsync(a => a.Code == updateArticleDto.Code && a.Id != id))
                {
                    return BadRequest($"Un article avec le code '{updateArticleDto.Code}' existe déjà.");
                }
                article.Code = updateArticleDto.Code;
            }

            if (!string.IsNullOrEmpty(updateArticleDto.Name))
                article.Name = updateArticleDto.Name;
            if (updateArticleDto.Description != null)
                article.Description = updateArticleDto.Description;
            if (updateArticleDto.Type.HasValue)
                article.Type = updateArticleDto.Type.Value;
            if (!string.IsNullOrEmpty(updateArticleDto.Unit))
                article.Unit = updateArticleDto.Unit;
            if (updateArticleDto.UnitPrice.HasValue)
                article.UnitPrice = updateArticleDto.UnitPrice.Value;
            if (updateArticleDto.Supplier != null)
                article.Supplier = updateArticleDto.Supplier;
            if (updateArticleDto.IsActive.HasValue)
                article.IsActive = updateArticleDto.IsActive.Value;

            article.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
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
        /// Supprime un article (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound($"Article avec l'ID {id} non trouvé.");
            }

            // Soft delete
            article.IsActive = false;
            article.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Recherche des articles par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> SearchArticles(
            [FromQuery] string? code,
            [FromQuery] string? name,
            [FromQuery] TypeArticle? type,
            [FromQuery] string? supplier,
            [FromQuery] bool activeOnly = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Articles.AsQueryable();

            if (activeOnly)
                query = query.Where(a => a.IsActive);

            if (!string.IsNullOrEmpty(code))
                query = query.Where(a => a.Code.Contains(code));

            if (!string.IsNullOrEmpty(name))
                query = query.Where(a => a.Name.Contains(name));

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            if (!string.IsNullOrEmpty(supplier))
                query = query.Where(a => a.Supplier != null && a.Supplier.Contains(supplier));

            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleDto
                {
                    Id = a.Id,
                    Code = a.Code,
                    Name = a.Name,
                    Description = a.Description,
                    Type = a.Type,
                    Unit = a.Unit,
                    UnitPrice = a.UnitPrice,
                    Supplier = a.Supplier,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(articles);
        }

        /// <summary>
        /// Récupère les articles par type
        /// </summary>
        [HttpGet("by-type/{type}")]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticlesByType(TypeArticle type)
        {
            var articles = await _context.Articles
                .Where(a => a.Type == type && a.IsActive)
                .Select(a => new ArticleDto
                {
                    Id = a.Id,
                    Code = a.Code,
                    Name = a.Name,
                    Description = a.Description,
                    Type = a.Type,
                    Unit = a.Unit,
                    UnitPrice = a.UnitPrice,
                    Supplier = a.Supplier,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(articles);
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
