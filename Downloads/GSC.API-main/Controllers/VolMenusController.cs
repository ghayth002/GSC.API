using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/vols/{volId}/menus")]
    [Authorize]
    public class VolMenusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VolMenusController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère les menus disponibles pour un vol (Admin uniquement)
        /// </summary>
        [HttpGet("available")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<IEnumerable<MenuDto>>> GetAvailableMenusForVol(int volId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {volId} non trouvé.");
            }

            // Récupérer les menus qui correspondent aux critères du vol et qui sont actifs
            var menus = await _context.Menus
                .Include(m => m.Fournisseur)
                    .ThenInclude(f => f!.User)
                .Include(m => m.MenuItems)
                    .ThenInclude(mi => mi.Article)
                .Where(m => m.IsActive && 
                           (m.Zone == null || m.Zone == vol.Zone) &&
                           (m.Season == null || m.Season == vol.Season) &&
                           m.FournisseurId != null) // Seulement les menus avec fournisseur
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

            foreach(var m in menus)
            {
                Console.WriteLine($"[DEBUG] Returning Menu: Id={m.Id}, Name='{m.Name}', Type='{m.TypePassager}'");
            }

            return Ok(menus);
        }

        /// <summary>
        /// Affecte un menu à un vol (Admin uniquement)
        /// </summary>
        [HttpPost("{menuId}/assign")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> AssignMenuToVol(int volId, int menuId, [FromBody] AssignMenuToVolDto assignDto)
        {
            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {volId} non trouvé.");
            }

            var menu = await _context.Menus
                .Include(m => m.Fournisseur)
                .FirstOrDefaultAsync(m => m.Id == menuId);
            if (menu == null)
            {
                return NotFound($"Menu avec l'ID {menuId} non trouvé.");
            }

            if (!menu.IsActive)
            {
                return BadRequest("Le menu sélectionné n'est pas actif.");
            }

            if (menu.FournisseurId == null)
            {
                return BadRequest("Le menu doit être associé à un fournisseur.");
            }

            // Vérifier si le menu n'est pas déjà assigné à ce vol avec ce type de passager
            var existingAssignment = await _context.MenusPlanHebergement
                .Include(mph => mph.PlanHebergement)
                .FirstOrDefaultAsync(mph => 
                    mph.MenuId == menuId && 
                    mph.PlanHebergement.VolId == volId &&
                    menu.TypePassager == assignDto.TypePassager);

            if (existingAssignment != null)
            {
                return BadRequest($"Ce menu est déjà assigné à ce vol pour le type de passager {assignDto.TypePassager}.");
            }

            // Récupérer ou créer le plan d'hébergement pour ce vol
            var planHebergement = await _context.PlansHebergement
                .FirstOrDefaultAsync(ph => ph.VolId == volId);

            if (planHebergement == null)
            {
                planHebergement = new PlanHebergement
                {
                    VolId = volId,
                    Name = await GeneratePlanHebergementNumero(volId),
                    Description = "Plan généré automatiquement pour l'assignation de menus",
                    Season = vol.Season ?? "Standard",
                    AircraftType = vol.Aircraft ?? "Unknown",
                    Zone = vol.Zone ?? "Unknown",
                    FlightDuration = vol.Duration,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PlansHebergement.Add(planHebergement);
                await _context.SaveChangesAsync();
            }

            // Créer l'association menu-plan d'hébergement
            var menuPlanHebergement = new MenuPlanHebergement
            {
                MenuId = menuId,
                PlanHebergementId = planHebergement.Id
            };

            _context.MenusPlanHebergement.Add(menuPlanHebergement);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Menu assigné au vol avec succès",
                planHebergementId = planHebergement.Id,
                menuId = menuId,
                volId = volId
            });
        }

        /// <summary>
        /// Récupère les menus assignés à un vol
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator,Manager,User")]
        public async Task<ActionResult<IEnumerable<MenuDto>>> GetVolMenus(int volId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {volId} non trouvé.");
            }

            var menus = await _context.MenusPlanHebergement
                .Include(mph => mph.Menu)
                    .ThenInclude(m => m.Fournisseur)
                        .ThenInclude(f => f!.User)
                .Include(mph => mph.Menu.MenuItems)
                    .ThenInclude(mi => mi.Article)
                .Include(mph => mph.PlanHebergement)
                .Where(mph => mph.PlanHebergement.VolId == volId)
                .Select(mph => new MenuDto
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
                })
                .ToListAsync();

            return Ok(menus);
        }

        /// <summary>
        /// Désassigne un menu d'un vol (Admin uniquement)
        /// </summary>
        [HttpDelete("{menuId}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> UnassignMenuFromVol(int volId, int menuId)
        {
            var menuPlanHebergement = await _context.MenusPlanHebergement
                .Include(mph => mph.PlanHebergement)
                .FirstOrDefaultAsync(mph => 
                    mph.MenuId == menuId && 
                    mph.PlanHebergement.VolId == volId);

            if (menuPlanHebergement == null)
            {
                return NotFound("Association menu-vol non trouvée.");
            }

            _context.MenusPlanHebergement.Remove(menuPlanHebergement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Menu désassigné du vol avec succès" });
        }

        /// <summary>
        /// Génère un numéro unique pour un plan d'hébergement
        /// </summary>
        private async Task<string> GeneratePlanHebergementNumero(int volId)
        {
            var vol = await _context.Vols.FindAsync(volId);
            var prefix = $"PH{vol!.FlightNumber}{vol.FlightDate:yyyyMMdd}";
            
            var lastPlan = await _context.PlansHebergement
                .Where(ph => ph.Name.StartsWith(prefix))
                .OrderByDescending(ph => ph.Name)
                .FirstOrDefaultAsync();

            if (lastPlan == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = lastPlan.Name.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var number))
            {
                return $"{prefix}{(number + 1):D3}";
            }

            return $"{prefix}001";
        }
    }

    /// <summary>
    /// DTO pour l'assignation d'un menu à un vol
    /// </summary>
    public class AssignMenuToVolDto
    {
        public string TypePassager { get; set; } = "Economy";
        public string? Commentaires { get; set; }
    }
}
