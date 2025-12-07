using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;
using System.Security.Claims;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Manager")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère les statistiques globales
        /// </summary>
        [HttpGet("global")]
        public async Task<ActionResult<GlobalStatisticsDto>> GetGlobalStatistics([FromQuery] AnalyticsFilterDto? filter = null)
        {
            var demandesQuery = _context.DemandesMenu.Where(d => d.IsActive);
            
            if (filter?.StartDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande >= filter.StartDate.Value);
            
            if (filter?.EndDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande <= filter.EndDate.Value);

            var totalDemandes = await demandesQuery.CountAsync();
            var demandesEnAttente = await demandesQuery.CountAsync(d => d.Status == StatusDemande.EnAttente);
            var demandesEnCours = await demandesQuery.CountAsync(d => d.Status == StatusDemande.EnCours);
            var demandesCompletees = await demandesQuery.CountAsync(d => d.Status == StatusDemande.Completee);
            var demandesAnnulees = await demandesQuery.CountAsync(d => d.Status == StatusDemande.Annulee);

            var totalMenus = await _context.Menus.CountAsync();
            var menusActifs = await _context.Menus.CountAsync(m => m.IsActive);
            
            var totalFournisseurs = await _context.Fournisseurs.CountAsync();
            var fournisseursActifs = await _context.Fournisseurs.CountAsync(f => f.IsActive);
            
            var totalVols = await _context.Vols.CountAsync();

            var tauxCompletion = totalDemandes > 0 
                ? Math.Round((decimal)demandesCompletees / totalDemandes * 100, 2) 
                : 0;

            return Ok(new GlobalStatisticsDto
            {
                TotalDemandes = totalDemandes,
                DemandesEnAttente = demandesEnAttente,
                DemandesEnCours = demandesEnCours,
                DemandesCompletees = demandesCompletees,
                DemandesAnnulees = demandesAnnulees,
                TotalMenus = totalMenus,
                MenusActifs = menusActifs,
                TotalFournisseurs = totalFournisseurs,
                FournisseursActifs = fournisseursActifs,
                TotalVols = totalVols,
                TauxCompletionDemandes = tauxCompletion
            });
        }

        /// <summary>
        /// Distribution des demandes par statut (Pie Chart)
        /// </summary>
        [HttpGet("demandes/status-distribution")]
        public async Task<ActionResult<List<DemandeStatusStatDto>>> GetStatusDistribution([FromQuery] AnalyticsFilterDto? filter = null)
        {
            var demandesQuery = _context.DemandesMenu.Where(d => d.IsActive);
            
            if (filter?.StartDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande >= filter.StartDate.Value);
            
            if (filter?.EndDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande <= filter.EndDate.Value);

            var totalDemandes = await demandesQuery.CountAsync();
            
            var statusGroups = await demandesQuery
                .GroupBy(d => d.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = statusGroups.Select(g => new DemandeStatusStatDto
            {
                Status = g.Status.ToString(),
                Count = g.Count,
                Percentage = totalDemandes > 0 ? Math.Round((decimal)g.Count / totalDemandes * 100, 2) : 0
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Distribution des demandes par type (Pie Chart)
        /// </summary>
        [HttpGet("demandes/type-distribution")]
        public async Task<ActionResult<List<DemandeTypeStatDto>>> GetTypeDistribution([FromQuery] AnalyticsFilterDto? filter = null)
        {
            var demandesQuery = _context.DemandesMenu.Where(d => d.IsActive);
            
            if (filter?.StartDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande >= filter.StartDate.Value);
            
            if (filter?.EndDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande <= filter.EndDate.Value);

            var totalDemandes = await demandesQuery.CountAsync();
            
            var typeGroups = await demandesQuery
                .GroupBy(d => d.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = typeGroups.Select(g => new DemandeTypeStatDto
            {
                Type = g.Type.ToString(),
                Count = g.Count,
                Percentage = totalDemandes > 0 ? Math.Round((decimal)g.Count / totalDemandes * 100, 2) : 0
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Performance des fournisseurs (Bar Chart)
        /// </summary>
        [HttpGet("fournisseurs/performance")]
        public async Task<ActionResult<List<FournisseurPerformanceDto>>> GetFournisseurPerformance(
            [FromQuery] int top = 10,
            [FromQuery] AnalyticsFilterDto? filter = null)
        {
            var demandesQuery = _context.DemandesMenu
                .Include(d => d.AssigneAFournisseur)
                .Where(d => d.IsActive && d.AssigneAFournisseurId != null);
            
            if (filter?.StartDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande >= filter.StartDate.Value);
            
            if (filter?.EndDate.HasValue == true)
                demandesQuery = demandesQuery.Where(d => d.DateDemande <= filter.EndDate.Value);

            var fournisseurStats = await _context.Fournisseurs
                .Where(f => f.IsActive)
                .Select(f => new
                {
                    Fournisseur = f,
                    Demandes = demandesQuery.Where(d => d.AssigneAFournisseurId == f.UserId).ToList()
                })
                .ToListAsync();

            var result = fournisseurStats
                .Where(fs => fs.Demandes.Any())
                .Select(fs =>
                {
                    var totalAssignees = fs.Demandes.Count;
                    var acceptees = fs.Demandes.Count(d => d.Status == StatusDemande.Acceptee || d.Status == StatusDemande.Completee);
                    var refusees = fs.Demandes.Count(d => d.Status == StatusDemande.Refusee);
                    var completees = fs.Demandes.Count(d => d.Status == StatusDemande.Completee);

                    var demandesAvecReponse = fs.Demandes.Where(d => d.DateReponse.HasValue).ToList();
                    var delaiMoyen = demandesAvecReponse.Any()
                        ? demandesAvecReponse.Average(d => (d.DateReponse!.Value - d.DateDemande).TotalHours)
                        : 0;

                    return new FournisseurPerformanceDto
                    {
                        FournisseurId = fs.Fournisseur.Id,
                        FournisseurName = fs.Fournisseur.CompanyName,
                        TotalDemandesAssignees = totalAssignees,
                        DemandesAcceptees = acceptees,
                        DemandesRefusees = refusees,
                        DemandesCompletees = completees,
                        TauxAcceptation = totalAssignees > 0 ? Math.Round((decimal)acceptees / totalAssignees * 100, 2) : 0,
                        TauxCompletion = totalAssignees > 0 ? Math.Round((decimal)completees / totalAssignees * 100, 2) : 0,
                        DelaiMoyenReponse = Math.Round((decimal)delaiMoyen, 2)
                    };
                })
                .OrderByDescending(f => f.TauxCompletion)
                .Take(top)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Tendances mensuelles (Line Chart)
        /// </summary>
        [HttpGet("trends/monthly")]
        public async Task<ActionResult<List<TrendDataDto>>> GetMonthlyTrends([FromQuery] int months = 12)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            
            var demandes = await _context.DemandesMenu
                .Where(d => d.IsActive && d.DateDemande >= startDate)
                .ToListAsync();

            var menus = await _context.Menus
                .Where(m => m.CreatedAt >= startDate)
                .ToListAsync();

            var monthlyData = Enumerable.Range(0, months)
                .Select(i =>
                {
                    var month = DateTime.UtcNow.AddMonths(-months + i + 1);
                    var monthStart = new DateTime(month.Year, month.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    return new TrendDataDto
                    {
                        Period = month.ToString("yyyy-MM"),
                        DemandesCreees = demandes.Count(d => d.DateDemande >= monthStart && d.DateDemande < monthEnd),
                        DemandesCompletees = demandes.Count(d => d.Status == StatusDemande.Completee && 
                                                                  d.DateReponse.HasValue &&
                                                                  d.DateReponse >= monthStart && 
                                                                  d.DateReponse < monthEnd),
                        MenusCrees = menus.Count(m => m.CreatedAt >= monthStart && m.CreatedAt < monthEnd)
                    };
                })
                .ToList();

            return Ok(monthlyData);
        }

        /// <summary>
        /// Statistiques des menus
        /// </summary>
        [HttpGet("menus/statistics")]
        public async Task<ActionResult<MenuAnalyticsDto>> GetMenuStatistics()
        {
            var totalMenus = await _context.Menus.CountAsync();
            var menusActifs = await _context.Menus.CountAsync(m => m.IsActive);
            var menusParFournisseur = await _context.Menus.Where(m => m.FournisseurId != null).CountAsync();

            var menusByType = await _context.Menus
                .GroupBy(m => m.TypePassager)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            var menusBySeason = await _context.Menus
                .Where(m => m.Season != null)
                .GroupBy(m => m.Season!)
                .Select(g => new { Season = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Season, x => x.Count);

            return Ok(new MenuAnalyticsDto
            {
                TotalMenus = totalMenus,
                MenusActifs = menusActifs,
                MenusParFournisseur = menusParFournisseur,
                MenusParTypePassager = menusByType,
                MenusParSaison = menusBySeason
            });
        }

        /// <summary>
        /// Rapport complet pour export PDF
        /// </summary>
        [HttpGet("report/complete")]
        public async Task<ActionResult<AnalyticsReportDto>> GetCompleteReport([FromQuery] AnalyticsFilterDto? filter = null)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var globalStats = await GetGlobalStatistics(filter);
            var statusDist = await GetStatusDistribution(filter);
            var typeDist = await GetTypeDistribution(filter);
            var topFournisseurs = await GetFournisseurPerformance(5, filter);
            var trends = await GetMonthlyTrends(6);
            var menuStats = await GetMenuStatistics();

            var report = new AnalyticsReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = $"{currentUser?.FirstName} {currentUser?.LastName}",
                GlobalStats = (globalStats.Result as OkObjectResult)?.Value as GlobalStatisticsDto ?? new(),
                StatusDistribution = (statusDist.Result as OkObjectResult)?.Value as List<DemandeStatusStatDto> ?? new(),
                TypeDistribution = (typeDist.Result as OkObjectResult)?.Value as List<DemandeTypeStatDto> ?? new(),
                TopFournisseurs = (topFournisseurs.Result as OkObjectResult)?.Value as List<FournisseurPerformanceDto> ?? new(),
                MonthlyTrends = (trends.Result as OkObjectResult)?.Value as List<TrendDataDto> ?? new(),
                MenuStats = (menuStats.Result as OkObjectResult)?.Value as MenuAnalyticsDto ?? new()
            };

            return Ok(report);
        }
    }
}
