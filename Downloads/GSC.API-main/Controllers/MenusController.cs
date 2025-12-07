using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenusController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les menus avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuDto>>> GetMenus([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var menus = await _context.Menus
                .Where(m => m.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MenuDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    TypePassager = m.TypePassager,
                    Season = m.Season,
                    Zone = m.Zone,
                    IsActive = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return Ok(menus);
        }

        /// <summary>
        /// Récupère un menu par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuDetailsDto>> GetMenu(int id)
        {
            var menu = await _context.Menus
                .Include(m => m.MenuItems)
                    .ThenInclude(mi => mi.Article)
                .Include(m => m.MenusPlanHebergement)
                    .ThenInclude(mph => mph.PlanHebergement)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null)
            {
                return NotFound($"Menu avec l'ID {id} non trouvé.");
            }

            var menuDto = new MenuDetailsDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description,
                TypePassager = menu.TypePassager,
                Season = menu.Season,
                Zone = menu.Zone,
                IsActive = menu.IsActive,
                CreatedAt = menu.CreatedAt,
                UpdatedAt = menu.UpdatedAt,
                MenuItems = menu.MenuItems.Select(mi => new MenuItemDto
                {
                    Id = mi.Id,
                    MenuId = mi.MenuId,
                    ArticleId = mi.ArticleId,
                    Quantity = mi.Quantity,
                    TypePassager = mi.TypePassager,
                    CreatedAt = mi.CreatedAt,
                    Article = new ArticleDto
                    {
                        Id = mi.Article.Id,
                        Code = mi.Article.Code,
                        Name = mi.Article.Name,
                        Description = mi.Article.Description,
                        Type = mi.Article.Type,
                        Unit = mi.Article.Unit,
                        UnitPrice = mi.Article.UnitPrice,
                        Supplier = mi.Article.Supplier,
                        IsActive = mi.Article.IsActive,
                        CreatedAt = mi.Article.CreatedAt,
                        UpdatedAt = mi.Article.UpdatedAt
                    }
                }).ToList(),
                PlansHebergement = menu.MenusPlanHebergement.Select(mph => new PlanHebergementDto
                {
                    Id = mph.PlanHebergement.Id,
                    VolId = mph.PlanHebergement.VolId,
                    Name = mph.PlanHebergement.Name,
                    Description = mph.PlanHebergement.Description,
                    Season = mph.PlanHebergement.Season,
                    AircraftType = mph.PlanHebergement.AircraftType,
                    Zone = mph.PlanHebergement.Zone,
                    FlightDuration = mph.PlanHebergement.FlightDuration,
                    IsActive = mph.PlanHebergement.IsActive,
                    CreatedAt = mph.PlanHebergement.CreatedAt,
                    UpdatedAt = mph.PlanHebergement.UpdatedAt
                }).ToList()
            };

            return Ok(menuDto);
        }

        /// <summary>
        /// Crée un nouveau menu avec ses items
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MenuDto>> CreateMenu(CreateMenuDto createMenuDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var menu = new Menu
                {
                    Name = createMenuDto.Name,
                    Description = createMenuDto.Description,
                    TypePassager = createMenuDto.TypePassager,
                    Season = createMenuDto.Season,
                    Zone = createMenuDto.Zone,
                    IsActive = createMenuDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();

                // Ajouter les items du menu
                foreach (var itemDto in createMenuDto.MenuItems)
                {
                    var menuItem = new MenuItem
                    {
                        MenuId = menu.Id,
                        ArticleId = itemDto.ArticleId,
                        Quantity = itemDto.Quantity,
                        TypePassager = itemDto.TypePassager,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.MenuItems.Add(menuItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var menuDto = new MenuDto
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Description = menu.Description,
                    TypePassager = menu.TypePassager,
                    Season = menu.Season,
                    Zone = menu.Zone,
                    IsActive = menu.IsActive,
                    CreatedAt = menu.CreatedAt,
                    UpdatedAt = menu.UpdatedAt
                };

                return CreatedAtAction(nameof(GetMenu), new { id = menu.Id }, menuDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Met à jour un menu existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenu(int id, UpdateMenuDto updateMenuDto)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound($"Menu avec l'ID {id} non trouvé.");
            }

            if (!string.IsNullOrEmpty(updateMenuDto.Name))
                menu.Name = updateMenuDto.Name;
            if (updateMenuDto.Description != null)
                menu.Description = updateMenuDto.Description;
            if (!string.IsNullOrEmpty(updateMenuDto.TypePassager))
                menu.TypePassager = updateMenuDto.TypePassager;
            if (updateMenuDto.Season != null)
                menu.Season = updateMenuDto.Season;
            if (updateMenuDto.Zone != null)
                menu.Zone = updateMenuDto.Zone;
            if (updateMenuDto.IsActive.HasValue)
                menu.IsActive = updateMenuDto.IsActive.Value;

            menu.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(id))
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
        /// Supprime un menu (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound($"Menu avec l'ID {id} non trouvé.");
            }

            menu.IsActive = false;
            menu.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Menu '{menu.Name}' désactivé avec succès." });
        }

        /// <summary>
        /// Supprime définitivement un menu et tous ses items (hard delete)
        /// </summary>
        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> DeleteMenuPermanent(int id)
        {
            var menu = await _context.Menus
                .Include(m => m.MenuItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null)
            {
                return NotFound($"Menu avec l'ID {id} non trouvé.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Supprimer d'abord tous les items du menu
                _context.MenuItems.RemoveRange(menu.MenuItems);

                // Supprimer le menu
                _context.Menus.Remove(menu);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Menu '{menu.Name}' supprimé définitivement avec {menu.MenuItems.Count} items.",
                    deletedId = id
                });
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return StatusCode(500, $"Erreur lors de la suppression: {ex.Message}");
            }
        }

        /// <summary>
        /// Ajoute un item à un menu
        /// </summary>
        [HttpPost("{menuId}/items")]
        public async Task<ActionResult<MenuItemDto>> AddMenuItem(int menuId, CreateMenuItemDto createMenuItemDto)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu == null)
            {
                return NotFound($"Menu avec l'ID {menuId} non trouvé.");
            }

            var article = await _context.Articles.FindAsync(createMenuItemDto.ArticleId);
            if (article == null)
            {
                return NotFound($"Article avec l'ID {createMenuItemDto.ArticleId} non trouvé.");
            }

            var menuItem = new MenuItem
            {
                MenuId = menuId,
                ArticleId = createMenuItemDto.ArticleId,
                Quantity = createMenuItemDto.Quantity,
                TypePassager = createMenuItemDto.TypePassager,
                CreatedAt = DateTime.UtcNow
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            var menuItemDto = new MenuItemDto
            {
                Id = menuItem.Id,
                MenuId = menuItem.MenuId,
                ArticleId = menuItem.ArticleId,
                Quantity = menuItem.Quantity,
                TypePassager = menuItem.TypePassager,
                CreatedAt = menuItem.CreatedAt,
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

            return Ok(menuItemDto);
        }

        /// <summary>
        /// Met à jour un item de menu
        /// </summary>
        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateMenuItem(int itemId, UpdateMenuItemDto updateMenuItemDto)
        {
            var menuItem = await _context.MenuItems.FindAsync(itemId);
            if (menuItem == null)
            {
                return NotFound($"Item de menu avec l'ID {itemId} non trouvé.");
            }

            if (updateMenuItemDto.Quantity.HasValue)
                menuItem.Quantity = updateMenuItemDto.Quantity.Value;
            if (updateMenuItemDto.TypePassager != null)
                menuItem.TypePassager = updateMenuItemDto.TypePassager;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(itemId))
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
        /// Supprime un item de menu
        /// </summary>
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> DeleteMenuItem(int itemId)
        {
            var menuItem = await _context.MenuItems.FindAsync(itemId);
            if (menuItem == null)
            {
                return NotFound($"Item de menu avec l'ID {itemId} non trouvé.");
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Recherche des menus par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MenuDto>>> SearchMenus(
            [FromQuery] string? name,
            [FromQuery] string? typePassager,
            [FromQuery] string? season,
            [FromQuery] string? zone,
            [FromQuery] bool activeOnly = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Menus.AsQueryable();

            if (activeOnly)
                query = query.Where(m => m.IsActive);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(m => m.Name.Contains(name));

            if (!string.IsNullOrEmpty(typePassager))
                query = query.Where(m => m.TypePassager.Contains(typePassager));

            if (!string.IsNullOrEmpty(season))
                query = query.Where(m => m.Season != null && m.Season.Contains(season));

            if (!string.IsNullOrEmpty(zone))
                query = query.Where(m => m.Zone != null && m.Zone.Contains(zone));

            var menus = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MenuDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    TypePassager = m.TypePassager,
                    Season = m.Season,
                    Zone = m.Zone,
                    IsActive = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return Ok(menus);
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.Id == id);
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}
