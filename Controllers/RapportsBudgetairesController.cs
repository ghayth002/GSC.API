using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RapportsBudgetairesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RapportsBudgetairesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les rapports budgétaires avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RapportBudgetaireDto>>> GetRapportsBudgetaires([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var rapports = await _context.RapportsBudgetaires
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RapportBudgetaireDto
                {
                    Id = r.Id,
                    Titre = r.Titre,
                    TypeRapport = r.TypeRapport,
                    DateDebut = r.DateDebut,
                    DateFin = r.DateFin,
                    DateGeneration = r.DateGeneration,
                    GenerePar = r.GenerePar,
                    NombreVolsTotal = r.NombreVolsTotal,
                    MontantPrevu = r.MontantPrevu,
                    MontantReel = r.MontantReel,
                    EcartMontant = r.EcartMontant,
                    PourcentageEcart = r.PourcentageEcart,
                    CoutRepas = r.CoutRepas,
                    CoutBoissons = r.CoutBoissons,
                    CoutConsommables = r.CoutConsommables,
                    CoutSemiConsommables = r.CoutSemiConsommables,
                    CoutMaterielDivers = r.CoutMaterielDivers,
                    Commentaires = r.Commentaires,
                    CheminFichier = r.CheminFichier,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(rapports);
        }

        /// <summary>
        /// Récupère un rapport budgétaire par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RapportBudgetaireDetailsDto>> GetRapportBudgetaire(int id)
        {
            var rapport = await _context.RapportsBudgetaires
                .Include(r => r.Details)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rapport == null)
            {
                return NotFound($"Rapport budgétaire avec l'ID {id} non trouvé.");
            }

            var rapportDto = new RapportBudgetaireDetailsDto
            {
                Id = rapport.Id,
                Titre = rapport.Titre,
                TypeRapport = rapport.TypeRapport,
                DateDebut = rapport.DateDebut,
                DateFin = rapport.DateFin,
                DateGeneration = rapport.DateGeneration,
                GenerePar = rapport.GenerePar,
                NombreVolsTotal = rapport.NombreVolsTotal,
                MontantPrevu = rapport.MontantPrevu,
                MontantReel = rapport.MontantReel,
                EcartMontant = rapport.EcartMontant,
                PourcentageEcart = rapport.PourcentageEcart,
                CoutRepas = rapport.CoutRepas,
                CoutBoissons = rapport.CoutBoissons,
                CoutConsommables = rapport.CoutConsommables,
                CoutSemiConsommables = rapport.CoutSemiConsommables,
                CoutMaterielDivers = rapport.CoutMaterielDivers,
                Commentaires = rapport.Commentaires,
                CheminFichier = rapport.CheminFichier,
                CreatedAt = rapport.CreatedAt,
                Details = rapport.Details.Select(d => new RapportBudgetaireDetailDto
                {
                    Id = d.Id,
                    RapportBudgetaireId = d.RapportBudgetaireId,
                    Categorie = d.Categorie,
                    Libelle = d.Libelle,
                    MontantPrevu = d.MontantPrevu,
                    MontantReel = d.MontantReel,
                    Ecart = d.Ecart,
                    PourcentageEcart = d.PourcentageEcart,
                    Quantite = d.Quantite
                }).ToList()
            };

            return Ok(rapportDto);
        }

        /// <summary>
        /// Génère un nouveau rapport budgétaire
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<RapportBudgetaireDto>> GenerateRapportBudgetaire(CreateRapportBudgetaireDto createRapportDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var rapport = new RapportBudgetaire
                {
                    Titre = createRapportDto.Titre,
                    TypeRapport = createRapportDto.TypeRapport,
                    DateDebut = createRapportDto.DateDebut,
                    DateFin = createRapportDto.DateFin,
                    DateGeneration = DateTime.UtcNow,
                    GenerePar = createRapportDto.GenerePar ?? HttpContext.User?.Identity?.Name,
                    Commentaires = createRapportDto.Commentaires,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RapportsBudgetaires.Add(rapport);
                await _context.SaveChangesAsync();

                // Générer les données du rapport
                await GenerateRapportData(rapport);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var rapportDto = new RapportBudgetaireDto
                {
                    Id = rapport.Id,
                    Titre = rapport.Titre,
                    TypeRapport = rapport.TypeRapport,
                    DateDebut = rapport.DateDebut,
                    DateFin = rapport.DateFin,
                    DateGeneration = rapport.DateGeneration,
                    GenerePar = rapport.GenerePar,
                    NombreVolsTotal = rapport.NombreVolsTotal,
                    MontantPrevu = rapport.MontantPrevu,
                    MontantReel = rapport.MontantReel,
                    EcartMontant = rapport.EcartMontant,
                    PourcentageEcart = rapport.PourcentageEcart,
                    CoutRepas = rapport.CoutRepas,
                    CoutBoissons = rapport.CoutBoissons,
                    CoutConsommables = rapport.CoutConsommables,
                    CoutSemiConsommables = rapport.CoutSemiConsommables,
                    CoutMaterielDivers = rapport.CoutMaterielDivers,
                    Commentaires = rapport.Commentaires,
                    CheminFichier = rapport.CheminFichier,
                    CreatedAt = rapport.CreatedAt
                };

                return CreatedAtAction(nameof(GetRapportBudgetaire), new { id = rapport.Id }, rapportDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Met à jour un rapport budgétaire existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRapportBudgetaire(int id, UpdateRapportBudgetaireDto updateRapportDto)
        {
            var rapport = await _context.RapportsBudgetaires.FindAsync(id);
            if (rapport == null)
            {
                return NotFound($"Rapport budgétaire avec l'ID {id} non trouvé.");
            }

            if (!string.IsNullOrEmpty(updateRapportDto.Titre))
                rapport.Titre = updateRapportDto.Titre;
            if (updateRapportDto.TypeRapport.HasValue)
                rapport.TypeRapport = updateRapportDto.TypeRapport.Value;
            if (updateRapportDto.DateDebut.HasValue)
                rapport.DateDebut = updateRapportDto.DateDebut.Value;
            if (updateRapportDto.DateFin.HasValue)
                rapport.DateFin = updateRapportDto.DateFin.Value;
            if (updateRapportDto.GenerePar != null)
                rapport.GenerePar = updateRapportDto.GenerePar;
            if (updateRapportDto.Commentaires != null)
                rapport.Commentaires = updateRapportDto.Commentaires;
            if (updateRapportDto.CheminFichier != null)
                rapport.CheminFichier = updateRapportDto.CheminFichier;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RapportBudgetaireExists(id))
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
        /// Supprime un rapport budgétaire
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRapportBudgetaire(int id)
        {
            var rapport = await _context.RapportsBudgetaires
                .Include(r => r.Details)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rapport == null)
            {
                return NotFound($"Rapport budgétaire avec l'ID {id} non trouvé.");
            }

            _context.RapportsBudgetaires.Remove(rapport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Génère un rapport de comparaison BCP vs BL pour une période
        /// </summary>
        [HttpPost("generate-comparison")]
        public async Task<ActionResult<object>> GenerateComparisonReport([FromQuery] DateTime dateDebut, [FromQuery] DateTime dateFin)
        {
            var vols = await _context.Vols
                .Include(v => v.BonsCommandePrevisionnels)
                    .ThenInclude(bcp => bcp.Lignes)
                        .ThenInclude(l => l.Article)
                .Include(v => v.BonsLivraison)
                    .ThenInclude(bl => bl.Lignes)
                        .ThenInclude(l => l.Article)
                .Where(v => v.FlightDate >= dateDebut && v.FlightDate <= dateFin)
                .ToListAsync();

            var ecarts = await _context.Ecarts
                .Include(e => e.Article)
                .Include(e => e.Vol)
                .Where(e => e.Vol.FlightDate >= dateDebut && e.Vol.FlightDate <= dateFin)
                .ToListAsync();

            var comparison = new
            {
                Periode = new { DateDebut = dateDebut, DateFin = dateFin },
                NombreVols = vols.Count,
                
                BonsCommandePrevisionnels = new
                {
                    Nombre = vols.SelectMany(v => v.BonsCommandePrevisionnels).Count(),
                    MontantTotal = vols.SelectMany(v => v.BonsCommandePrevisionnels).Sum(bcp => bcp.MontantTotal)
                },
                
                BonsLivraison = new
                {
                    Nombre = vols.SelectMany(v => v.BonsLivraison).Count(),
                    NombreValides = vols.SelectMany(v => v.BonsLivraison).Count(bl => bl.Status == StatusBL.Valide),
                    MontantTotal = vols.SelectMany(v => v.BonsLivraison).Where(bl => bl.Status == StatusBL.Valide).Sum(bl => bl.MontantTotal)
                },
                
                Ecarts = new
                {
                    NombreTotal = ecarts.Count,
                    EnAttente = ecarts.Count(e => e.Status == StatusEcart.EnAttente),
                    Resolus = ecarts.Count(e => e.Status == StatusEcart.Resolu),
                    MontantTotal = ecarts.Sum(e => Math.Abs(e.EcartMontant)),
                    ParType = ecarts.GroupBy(e => e.TypeEcart).Select(g => new
                    {
                        Type = g.Key.ToString(),
                        Nombre = g.Count(),
                        Montant = g.Sum(e => Math.Abs(e.EcartMontant))
                    }).ToList()
                },
                
                CoutsParTypeArticle = vols.SelectMany(v => v.BonsLivraison)
                    .Where(bl => bl.Status == StatusBL.Valide)
                    .SelectMany(bl => bl.Lignes)
                    .GroupBy(l => l.Article.Type)
                    .Select(g => new
                    {
                        Type = g.Key.ToString(),
                        Quantite = g.Sum(l => l.QuantiteLivree),
                        Montant = g.Sum(l => l.MontantLigne)
                    })
                    .ToList(),
                
                TopArticles = vols.SelectMany(v => v.BonsLivraison)
                    .Where(bl => bl.Status == StatusBL.Valide)
                    .SelectMany(bl => bl.Lignes)
                    .GroupBy(l => new { l.Article.Code, l.Article.Name })
                    .Select(g => new
                    {
                        Code = g.Key.Code,
                        Nom = g.Key.Name,
                        QuantiteTotal = g.Sum(l => l.QuantiteLivree),
                        MontantTotal = g.Sum(l => l.MontantLigne)
                    })
                    .OrderByDescending(x => x.MontantTotal)
                    .Take(10)
                    .ToList()
            };

            return Ok(comparison);
        }

        /// <summary>
        /// Génère un rapport de performance par zone/destination
        /// </summary>
        [HttpGet("performance-by-zone")]
        public async Task<ActionResult<object>> GetPerformanceByZone([FromQuery] DateTime? dateDebut, [FromQuery] DateTime? dateFin)
        {
            var query = _context.Vols.AsQueryable();

            if (dateDebut.HasValue)
                query = query.Where(v => v.FlightDate >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(v => v.FlightDate <= dateFin.Value);

            var vols = await query
                .Include(v => v.BonsLivraison)
                .ToListAsync();

            var ecartsByVol = await _context.Ecarts
                .Where(e => dateDebut == null || e.Vol.FlightDate >= dateDebut.Value)
                .Where(e => dateFin == null || e.Vol.FlightDate <= dateFin.Value)
                .GroupBy(e => e.VolId)
                .ToDictionaryAsync(g => g.Key, g => new
                {
                    Nombre = g.Count(),
                    Montant = g.Sum(e => Math.Abs(e.EcartMontant))
                });

            var performanceByZone = vols
                .GroupBy(v => v.Zone)
                .Select(g => new
                {
                    Zone = g.Key,
                    NombreVols = g.Count(),
                    PassagersTotal = g.Sum(v => v.ActualPassengers),
                    CoutTotal = g.SelectMany(v => v.BonsLivraison)
                        .Where(bl => bl.Status == StatusBL.Valide)
                        .Sum(bl => bl.MontantTotal),
                    CoutMoyenParVol = g.SelectMany(v => v.BonsLivraison)
                        .Where(bl => bl.Status == StatusBL.Valide)
                        .Sum(bl => bl.MontantTotal) / Math.Max(g.Count(), 1),
                    CoutMoyenParPassager = g.Sum(v => v.ActualPassengers) > 0 
                        ? g.SelectMany(v => v.BonsLivraison)
                            .Where(bl => bl.Status == StatusBL.Valide)
                            .Sum(bl => bl.MontantTotal) / g.Sum(v => v.ActualPassengers)
                        : 0,
                    NombreEcarts = g.Sum(v => ecartsByVol.ContainsKey(v.Id) ? ecartsByVol[v.Id].Nombre : 0),
                    MontantEcarts = g.Sum(v => ecartsByVol.ContainsKey(v.Id) ? ecartsByVol[v.Id].Montant : 0)
                })
                .OrderByDescending(x => x.CoutTotal)
                .ToList();

            return Ok(performanceByZone);
        }

        /// <summary>
        /// Génère un rapport de tendance mensuelle
        /// </summary>
        [HttpGet("monthly-trends")]
        public async Task<ActionResult<object>> GetMonthlyTrends([FromQuery] int year)
        {
            var vols = await _context.Vols
                .Include(v => v.BonsCommandePrevisionnels)
                .Include(v => v.BonsLivraison)
                .Where(v => v.FlightDate.Year == year)
                .ToListAsync();

            var ecarts = await _context.Ecarts
                .Include(e => e.Vol)
                .Where(e => e.Vol.FlightDate.Year == year)
                .ToListAsync();

            var monthlyData = Enumerable.Range(1, 12)
                .Select(month => new
                {
                    Mois = month,
                    NomMois = new DateTime(year, month, 1).ToString("MMMM"),
                    NombreVols = vols.Count(v => v.FlightDate.Month == month),
                    PassagersTotal = vols.Where(v => v.FlightDate.Month == month).Sum(v => v.ActualPassengers),
                    MontantBCP = vols.Where(v => v.FlightDate.Month == month)
                        .SelectMany(v => v.BonsCommandePrevisionnels).Sum(bcp => bcp.MontantTotal),
                    MontantBL = vols.Where(v => v.FlightDate.Month == month)
                        .SelectMany(v => v.BonsLivraison)
                        .Where(bl => bl.Status == StatusBL.Valide)
                        .Sum(bl => bl.MontantTotal),
                    NombreEcarts = ecarts.Count(e => e.Vol.FlightDate.Month == month),
                    MontantEcarts = ecarts.Where(e => e.Vol.FlightDate.Month == month)
                        .Sum(e => Math.Abs(e.EcartMontant))
                })
                .ToList();

            return Ok(new
            {
                Annee = year,
                DonneesMensuelles = monthlyData,
                Totaux = new
                {
                    NombreVolsTotal = monthlyData.Sum(m => m.NombreVols),
                    PassagersTotalAnnuel = monthlyData.Sum(m => m.PassagersTotal),
                    MontantBCPTotal = monthlyData.Sum(m => m.MontantBCP),
                    MontantBLTotal = monthlyData.Sum(m => m.MontantBL),
                    EcartTotal = monthlyData.Sum(m => m.MontantBCP) - monthlyData.Sum(m => m.MontantBL),
                    NombreEcartsTotal = monthlyData.Sum(m => m.NombreEcarts),
                    MontantEcartsTotal = monthlyData.Sum(m => m.MontantEcarts)
                }
            });
        }

        /// <summary>
        /// Génère les données du rapport budgétaire
        /// </summary>
        private async Task GenerateRapportData(RapportBudgetaire rapport)
        {
            // Récupérer les données de la période
            var vols = await _context.Vols
                .Include(v => v.BonsCommandePrevisionnels)
                    .ThenInclude(bcp => bcp.Lignes)
                        .ThenInclude(l => l.Article)
                .Include(v => v.BonsLivraison)
                    .ThenInclude(bl => bl.Lignes)
                        .ThenInclude(l => l.Article)
                .Where(v => v.FlightDate >= rapport.DateDebut && v.FlightDate <= rapport.DateFin)
                .ToListAsync();

            var ecarts = await _context.Ecarts
                .Include(e => e.Vol)
                .Where(e => e.Vol.FlightDate >= rapport.DateDebut && e.Vol.FlightDate <= rapport.DateFin)
                .ToListAsync();

            // Calculer les totaux
            rapport.NombreVolsTotal = vols.Count;
            rapport.MontantPrevu = vols.SelectMany(v => v.BonsCommandePrevisionnels).Sum(bcp => bcp.MontantTotal);
            rapport.MontantReel = vols.SelectMany(v => v.BonsLivraison)
                .Where(bl => bl.Status == StatusBL.Valide).Sum(bl => bl.MontantTotal);
            rapport.EcartMontant = rapport.MontantReel - rapport.MontantPrevu;
            rapport.PourcentageEcart = rapport.MontantPrevu != 0 
                ? (rapport.EcartMontant / rapport.MontantPrevu) * 100 
                : 0;

            // Calculer les coûts par type d'article
            var coutsByType = vols.SelectMany(v => v.BonsLivraison)
                .Where(bl => bl.Status == StatusBL.Valide)
                .SelectMany(bl => bl.Lignes)
                .GroupBy(l => l.Article.Type)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.MontantLigne));

            rapport.CoutRepas = coutsByType.GetValueOrDefault(TypeArticle.Repas, 0);
            rapport.CoutBoissons = coutsByType.GetValueOrDefault(TypeArticle.Boisson, 0);
            rapport.CoutConsommables = coutsByType.GetValueOrDefault(TypeArticle.Consommable, 0);
            rapport.CoutSemiConsommables = coutsByType.GetValueOrDefault(TypeArticle.SemiConsommable, 0);
            rapport.CoutMaterielDivers = coutsByType.GetValueOrDefault(TypeArticle.MaterielDivers, 0);

            // Créer les détails du rapport
            await CreateRapportDetails(rapport, vols, ecarts);
        }

        /// <summary>
        /// Crée les détails du rapport budgétaire
        /// </summary>
        private async Task CreateRapportDetails(RapportBudgetaire rapport, List<Vol> vols, List<Ecart> ecarts)
        {
            var details = new List<RapportBudgetaireDetail>();

            // Détails par zone
            var zoneDetails = vols.GroupBy(v => v.Zone)
                .Select(g => new RapportBudgetaireDetail
                {
                    RapportBudgetaireId = rapport.Id,
                    Categorie = "Zone",
                    Libelle = g.Key,
                    MontantPrevu = g.SelectMany(v => v.BonsCommandePrevisionnels).Sum(bcp => bcp.MontantTotal),
                    MontantReel = g.SelectMany(v => v.BonsLivraison)
                        .Where(bl => bl.Status == StatusBL.Valide).Sum(bl => bl.MontantTotal),
                    Quantite = g.Count()
                });

            details.AddRange(zoneDetails);

            // Détails par type d'article
            var articleTypeDetails = vols.SelectMany(v => v.BonsLivraison)
                .Where(bl => bl.Status == StatusBL.Valide)
                .SelectMany(bl => bl.Lignes)
                .GroupBy(l => l.Article.Type)
                .Select(g => new RapportBudgetaireDetail
                {
                    RapportBudgetaireId = rapport.Id,
                    Categorie = "Type Article",
                    Libelle = g.Key.ToString(),
                    MontantPrevu = 0, // À calculer si nécessaire
                    MontantReel = g.Sum(l => l.MontantLigne),
                    Quantite = g.Sum(l => l.QuantiteLivree)
                });

            details.AddRange(articleTypeDetails);

            // Calculer les écarts pour chaque détail
            foreach (var detail in details)
            {
                detail.Ecart = detail.MontantReel - detail.MontantPrevu;
                detail.PourcentageEcart = detail.MontantPrevu != 0 
                    ? (detail.Ecart / detail.MontantPrevu) * 100 
                    : 0;
            }

            _context.RapportBudgetaireDetails.AddRange(details);
        }

        /// <summary>
        /// Recherche des rapports budgétaires par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RapportBudgetaireDto>>> SearchRapports(
            [FromQuery] string? titre,
            [FromQuery] TypeRapport? typeRapport,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? generePar,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.RapportsBudgetaires.AsQueryable();

            if (!string.IsNullOrEmpty(titre))
                query = query.Where(r => r.Titre.Contains(titre));

            if (typeRapport.HasValue)
                query = query.Where(r => r.TypeRapport == typeRapport.Value);

            if (dateDebut.HasValue)
                query = query.Where(r => r.DateDebut >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(r => r.DateFin <= dateFin.Value);

            if (!string.IsNullOrEmpty(generePar))
                query = query.Where(r => r.GenerePar != null && r.GenerePar.Contains(generePar));

            var rapports = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RapportBudgetaireDto
                {
                    Id = r.Id,
                    Titre = r.Titre,
                    TypeRapport = r.TypeRapport,
                    DateDebut = r.DateDebut,
                    DateFin = r.DateFin,
                    DateGeneration = r.DateGeneration,
                    GenerePar = r.GenerePar,
                    NombreVolsTotal = r.NombreVolsTotal,
                    MontantPrevu = r.MontantPrevu,
                    MontantReel = r.MontantReel,
                    EcartMontant = r.EcartMontant,
                    PourcentageEcart = r.PourcentageEcart,
                    CoutRepas = r.CoutRepas,
                    CoutBoissons = r.CoutBoissons,
                    CoutConsommables = r.CoutConsommables,
                    CoutSemiConsommables = r.CoutSemiConsommables,
                    CoutMaterielDivers = r.CoutMaterielDivers,
                    Commentaires = r.Commentaires,
                    CheminFichier = r.CheminFichier,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(rapports);
        }

        private bool RapportBudgetaireExists(int id)
        {
            return _context.RapportsBudgetaires.Any(e => e.Id == id);
        }
    }
}
