using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VolsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les vols avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VolDto>>> GetVols([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var vols = await _context.Vols
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VolDto
                {
                    Id = v.Id,
                    FlightNumber = v.FlightNumber,
                    FlightDate = v.FlightDate,
                    DepartureTime = v.DepartureTime,
                    ArrivalTime = v.ArrivalTime,
                    Aircraft = v.Aircraft,
                    Origin = v.Origin,
                    Destination = v.Destination,
                    Zone = v.Zone,
                    EstimatedPassengers = v.EstimatedPassengers,
                    ActualPassengers = v.ActualPassengers,
                    Duration = v.Duration,
                    Season = v.Season,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();

            return Ok(vols);
        }

        /// <summary>
        /// Récupère un vol par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<VolDetailsDto>> GetVol(int id)
        {
            var vol = await _context.Vols
                .Include(v => v.PlanHebergement)
                .Include(v => v.BonsCommandePrevisionnels)
                .Include(v => v.BonsLivraison)
                .Include(v => v.VolBoitesMedicales)
                    .ThenInclude(vbm => vbm.BoiteMedicale)
                .Include(v => v.DossierVol)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {id} non trouvé.");
            }

            var volDto = new VolDetailsDto
            {
                Id = vol.Id,
                FlightNumber = vol.FlightNumber,
                FlightDate = vol.FlightDate,
                DepartureTime = vol.DepartureTime,
                ArrivalTime = vol.ArrivalTime,
                Aircraft = vol.Aircraft,
                Origin = vol.Origin,
                Destination = vol.Destination,
                Zone = vol.Zone,
                EstimatedPassengers = vol.EstimatedPassengers,
                ActualPassengers = vol.ActualPassengers,
                Duration = vol.Duration,
                Season = vol.Season,
                CreatedAt = vol.CreatedAt,
                UpdatedAt = vol.UpdatedAt,
                PlanHebergement = vol.PlanHebergement != null ? new PlanHebergementDto
                {
                    Id = vol.PlanHebergement.Id,
                    VolId = vol.PlanHebergement.VolId,
                    Name = vol.PlanHebergement.Name,
                    Description = vol.PlanHebergement.Description,
                    Season = vol.PlanHebergement.Season,
                    AircraftType = vol.PlanHebergement.AircraftType,
                    Zone = vol.PlanHebergement.Zone,
                    FlightDuration = vol.PlanHebergement.FlightDuration,
                    IsActive = vol.PlanHebergement.IsActive,
                    CreatedAt = vol.PlanHebergement.CreatedAt,
                    UpdatedAt = vol.PlanHebergement.UpdatedAt
                } : null,
                BonsCommandePrevisionnels = vol.BonsCommandePrevisionnels.Select(bcp => new BonCommandePrevisionnelDto
                {
                    Id = bcp.Id,
                    Numero = bcp.Numero,
                    VolId = bcp.VolId,
                    DateCommande = bcp.DateCommande,
                    Status = bcp.Status,
                    Fournisseur = bcp.Fournisseur,
                    MontantTotal = bcp.MontantTotal,
                    Commentaires = bcp.Commentaires,
                    CreatedAt = bcp.CreatedAt,
                    UpdatedAt = bcp.UpdatedAt,
                    CreatedBy = bcp.CreatedBy
                }).ToList(),
                BonsLivraison = vol.BonsLivraison.Select(bl => new BonLivraisonDto
                {
                    Id = bl.Id,
                    Numero = bl.Numero,
                    VolId = bl.VolId,
                    BonCommandePrevisionnelId = bl.BonCommandePrevisionnelId,
                    DateLivraison = bl.DateLivraison,
                    Status = bl.Status,
                    Fournisseur = bl.Fournisseur,
                    Livreur = bl.Livreur,
                    Commentaires = bl.Commentaires,
                    MontantTotal = bl.MontantTotal,
                    CreatedAt = bl.CreatedAt,
                    UpdatedAt = bl.UpdatedAt,
                    ValidatedBy = bl.ValidatedBy,
                    ValidationDate = bl.ValidationDate
                }).ToList(),
                VolBoitesMedicales = vol.VolBoitesMedicales.Select(vbm => new VolBoiteMedicaleDto
                {
                    Id = vbm.Id,
                    VolId = vbm.VolId,
                    BoiteMedicaleId = vbm.BoiteMedicaleId,
                    DateAssignation = vbm.DateAssignation,
                    AssignePar = vbm.AssignePar,
                    Commentaires = vbm.Commentaires,
                    Vol = new VolDto(),
                    BoiteMedicale = new BoiteMedicaleDto
                    {
                        Id = vbm.BoiteMedicale.Id,
                        Numero = vbm.BoiteMedicale.Numero,
                        Name = vbm.BoiteMedicale.Name,
                        Type = vbm.BoiteMedicale.Type,
                        Status = vbm.BoiteMedicale.Status,
                        Description = vbm.BoiteMedicale.Description,
                        DateExpiration = vbm.BoiteMedicale.DateExpiration,
                        DerniereMaintenance = vbm.BoiteMedicale.DerniereMaintenance,
                        ProchaineMaintenance = vbm.BoiteMedicale.ProchaineMaintenance,
                        ResponsableMaintenance = vbm.BoiteMedicale.ResponsableMaintenance,
                        IsActive = vbm.BoiteMedicale.IsActive,
                        CreatedAt = vbm.BoiteMedicale.CreatedAt,
                        UpdatedAt = vbm.BoiteMedicale.UpdatedAt
                    }
                }).ToList(),
                DossierVol = vol.DossierVol != null ? new DossierVolDto
                {
                    Id = vol.DossierVol.Id,
                    VolId = vol.DossierVol.VolId,
                    Numero = vol.DossierVol.Numero,
                    Status = vol.DossierVol.Status,
                    DateCreation = vol.DossierVol.DateCreation,
                    DateValidation = vol.DossierVol.DateValidation,
                    ValidePar = vol.DossierVol.ValidePar,
                    Resume = vol.DossierVol.Resume,
                    Commentaires = vol.DossierVol.Commentaires,
                    CoutTotal = vol.DossierVol.CoutTotal,
                    NombreEcarts = vol.DossierVol.NombreEcarts,
                    MontantEcarts = vol.DossierVol.MontantEcarts,
                    CreatedAt = vol.DossierVol.CreatedAt,
                    UpdatedAt = vol.DossierVol.UpdatedAt
                } : null
            };

            return Ok(volDto);
        }

        /// <summary>
        /// Crée un nouveau vol
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<VolDto>> CreateVol(CreateVolDto createVolDto)
        {
            var vol = new Vol
            {
                FlightNumber = createVolDto.FlightNumber,
                FlightDate = createVolDto.FlightDate,
                DepartureTime = createVolDto.DepartureTime,
                ArrivalTime = createVolDto.ArrivalTime,
                Aircraft = createVolDto.Aircraft,
                Origin = createVolDto.Origin,
                Destination = createVolDto.Destination,
                Zone = createVolDto.Zone,
                EstimatedPassengers = createVolDto.EstimatedPassengers,
                ActualPassengers = createVolDto.ActualPassengers,
                Duration = createVolDto.Duration,
                Season = createVolDto.Season,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vols.Add(vol);
            await _context.SaveChangesAsync();

            var volDto = new VolDto
            {
                Id = vol.Id,
                FlightNumber = vol.FlightNumber,
                FlightDate = vol.FlightDate,
                DepartureTime = vol.DepartureTime,
                ArrivalTime = vol.ArrivalTime,
                Aircraft = vol.Aircraft,
                Origin = vol.Origin,
                Destination = vol.Destination,
                Zone = vol.Zone,
                EstimatedPassengers = vol.EstimatedPassengers,
                ActualPassengers = vol.ActualPassengers,
                Duration = vol.Duration,
                Season = vol.Season,
                CreatedAt = vol.CreatedAt,
                UpdatedAt = vol.UpdatedAt
            };

            return CreatedAtAction(nameof(GetVol), new { id = vol.Id }, volDto);
        }

        /// <summary>
        /// Met à jour un vol existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVol(int id, UpdateVolDto updateVolDto)
        {
            var vol = await _context.Vols.FindAsync(id);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {id} non trouvé.");
            }

            if (!string.IsNullOrEmpty(updateVolDto.FlightNumber))
                vol.FlightNumber = updateVolDto.FlightNumber;
            if (updateVolDto.FlightDate.HasValue)
                vol.FlightDate = updateVolDto.FlightDate.Value;
            if (updateVolDto.DepartureTime.HasValue)
                vol.DepartureTime = updateVolDto.DepartureTime.Value;
            if (updateVolDto.ArrivalTime.HasValue)
                vol.ArrivalTime = updateVolDto.ArrivalTime.Value;
            if (!string.IsNullOrEmpty(updateVolDto.Aircraft))
                vol.Aircraft = updateVolDto.Aircraft;
            if (!string.IsNullOrEmpty(updateVolDto.Origin))
                vol.Origin = updateVolDto.Origin;
            if (!string.IsNullOrEmpty(updateVolDto.Destination))
                vol.Destination = updateVolDto.Destination;
            if (!string.IsNullOrEmpty(updateVolDto.Zone))
                vol.Zone = updateVolDto.Zone;
            if (updateVolDto.EstimatedPassengers.HasValue)
                vol.EstimatedPassengers = updateVolDto.EstimatedPassengers.Value;
            if (updateVolDto.ActualPassengers.HasValue)
                vol.ActualPassengers = updateVolDto.ActualPassengers.Value;
            if (updateVolDto.Duration.HasValue)
                vol.Duration = updateVolDto.Duration.Value;
            if (!string.IsNullOrEmpty(updateVolDto.Season))
                vol.Season = updateVolDto.Season;

            vol.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VolExists(id))
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
        /// Supprime un vol
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVol(int id)
        {
            var vol = await _context.Vols.FindAsync(id);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {id} non trouvé.");
            }

            _context.Vols.Remove(vol);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Recherche des vols par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VolDto>>> SearchVols(
            [FromQuery] string? flightNumber,
            [FromQuery] DateTime? flightDate,
            [FromQuery] string? origin,
            [FromQuery] string? destination,
            [FromQuery] string? zone,
            [FromQuery] string? aircraft,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Vols.AsQueryable();

            if (!string.IsNullOrEmpty(flightNumber))
                query = query.Where(v => v.FlightNumber.Contains(flightNumber));

            if (flightDate.HasValue)
                query = query.Where(v => v.FlightDate.Date == flightDate.Value.Date);

            if (!string.IsNullOrEmpty(origin))
                query = query.Where(v => v.Origin.Contains(origin));

            if (!string.IsNullOrEmpty(destination))
                query = query.Where(v => v.Destination.Contains(destination));

            if (!string.IsNullOrEmpty(zone))
                query = query.Where(v => v.Zone.Contains(zone));

            if (!string.IsNullOrEmpty(aircraft))
                query = query.Where(v => v.Aircraft.Contains(aircraft));

            var vols = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VolDto
                {
                    Id = v.Id,
                    FlightNumber = v.FlightNumber,
                    FlightDate = v.FlightDate,
                    DepartureTime = v.DepartureTime,
                    ArrivalTime = v.ArrivalTime,
                    Aircraft = v.Aircraft,
                    Origin = v.Origin,
                    Destination = v.Destination,
                    Zone = v.Zone,
                    EstimatedPassengers = v.EstimatedPassengers,
                    ActualPassengers = v.ActualPassengers,
                    Duration = v.Duration,
                    Season = v.Season,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();

            return Ok(vols);
        }

        private bool VolExists(int id)
        {
            return _context.Vols.Any(e => e.Id == id);
        }
    }
}
