using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoitesMedicalesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BoitesMedicalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère toutes les boîtes médicales avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoiteMedicaleDto>>> GetBoitesMedicales([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var boites = await _context.BoitesMedicales
                .Where(b => b.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BoiteMedicaleDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    Name = b.Name,
                    Type = b.Type,
                    Status = b.Status,
                    Description = b.Description,
                    DateExpiration = b.DateExpiration,
                    DerniereMaintenance = b.DerniereMaintenance,
                    ProchaineMaintenance = b.ProchaineMaintenance,
                    ResponsableMaintenance = b.ResponsableMaintenance,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync();

            return Ok(boites);
        }

        /// <summary>
        /// Récupère une boîte médicale par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BoiteMedicaleDetailsDto>> GetBoiteMedicale(int id)
        {
            var boite = await _context.BoitesMedicales
                .Include(b => b.Items)
                .Include(b => b.VolBoitesMedicales)
                    .ThenInclude(vbm => vbm.Vol)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boite == null)
            {
                return NotFound($"Boîte médicale avec l'ID {id} non trouvée.");
            }

            var boiteDto = new BoiteMedicaleDetailsDto
            {
                Id = boite.Id,
                Numero = boite.Numero,
                Name = boite.Name,
                Type = boite.Type,
                Status = boite.Status,
                Description = boite.Description,
                DateExpiration = boite.DateExpiration,
                DerniereMaintenance = boite.DerniereMaintenance,
                ProchaineMaintenance = boite.ProchaineMaintenance,
                ResponsableMaintenance = boite.ResponsableMaintenance,
                IsActive = boite.IsActive,
                CreatedAt = boite.CreatedAt,
                UpdatedAt = boite.UpdatedAt,
                Items = boite.Items.Select(i => new BoiteMedicaleItemDto
                {
                    Id = i.Id,
                    BoiteMedicaleId = i.BoiteMedicaleId,
                    Name = i.Name,
                    Description = i.Description,
                    Quantite = i.Quantite,
                    Unite = i.Unite,
                    DateExpiration = i.DateExpiration,
                    Fabricant = i.Fabricant,
                    NumeroLot = i.NumeroLot
                }).ToList(),
                VolBoitesMedicales = boite.VolBoitesMedicales.Select(vbm => new VolBoiteMedicaleDto
                {
                    Id = vbm.Id,
                    VolId = vbm.VolId,
                    BoiteMedicaleId = vbm.BoiteMedicaleId,
                    DateAssignation = vbm.DateAssignation,
                    AssignePar = vbm.AssignePar,
                    Commentaires = vbm.Commentaires,
                    Vol = new VolDto
                    {
                        Id = vbm.Vol.Id,
                        FlightNumber = vbm.Vol.FlightNumber,
                        FlightDate = vbm.Vol.FlightDate,
                        DepartureTime = vbm.Vol.DepartureTime,
                        ArrivalTime = vbm.Vol.ArrivalTime,
                        Aircraft = vbm.Vol.Aircraft,
                        Origin = vbm.Vol.Origin,
                        Destination = vbm.Vol.Destination,
                        Zone = vbm.Vol.Zone,
                        EstimatedPassengers = vbm.Vol.EstimatedPassengers,
                        ActualPassengers = vbm.Vol.ActualPassengers,
                        Duration = vbm.Vol.Duration,
                        Season = vbm.Vol.Season,
                        CreatedAt = vbm.Vol.CreatedAt,
                        UpdatedAt = vbm.Vol.UpdatedAt
                    },
                    BoiteMedicale = new BoiteMedicaleDto()
                }).ToList()
            };

            return Ok(boiteDto);
        }

        /// <summary>
        /// Crée une nouvelle boîte médicale avec ses items
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BoiteMedicaleDto>> CreateBoiteMedicale(CreateBoiteMedicaleDto createBoiteDto)
        {
            // Vérifier l'unicité du numéro
            if (await _context.BoitesMedicales.AnyAsync(b => b.Numero == createBoiteDto.Numero))
            {
                return BadRequest($"Une boîte médicale avec le numéro '{createBoiteDto.Numero}' existe déjà.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var boite = new BoiteMedicale
                {
                    Numero = createBoiteDto.Numero,
                    Name = createBoiteDto.Name,
                    Type = createBoiteDto.Type,
                    Status = StatusBoiteMedicale.Disponible,
                    Description = createBoiteDto.Description,
                    DateExpiration = createBoiteDto.DateExpiration,
                    DerniereMaintenance = createBoiteDto.DerniereMaintenance,
                    ProchaineMaintenance = createBoiteDto.ProchaineMaintenance,
                    ResponsableMaintenance = createBoiteDto.ResponsableMaintenance,
                    IsActive = createBoiteDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BoitesMedicales.Add(boite);
                await _context.SaveChangesAsync();

                // Ajouter les items de la boîte
                foreach (var itemDto in createBoiteDto.Items)
                {
                    var item = new BoiteMedicaleItem
                    {
                        BoiteMedicaleId = boite.Id,
                        Name = itemDto.Name,
                        Description = itemDto.Description,
                        Quantite = itemDto.Quantite,
                        Unite = itemDto.Unite,
                        DateExpiration = itemDto.DateExpiration,
                        Fabricant = itemDto.Fabricant,
                        NumeroLot = itemDto.NumeroLot
                    };

                    _context.BoiteMedicaleItems.Add(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var boiteDto = new BoiteMedicaleDto
                {
                    Id = boite.Id,
                    Numero = boite.Numero,
                    Name = boite.Name,
                    Type = boite.Type,
                    Status = boite.Status,
                    Description = boite.Description,
                    DateExpiration = boite.DateExpiration,
                    DerniereMaintenance = boite.DerniereMaintenance,
                    ProchaineMaintenance = boite.ProchaineMaintenance,
                    ResponsableMaintenance = boite.ResponsableMaintenance,
                    IsActive = boite.IsActive,
                    CreatedAt = boite.CreatedAt,
                    UpdatedAt = boite.UpdatedAt
                };

                return CreatedAtAction(nameof(GetBoiteMedicale), new { id = boite.Id }, boiteDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Met à jour une boîte médicale existante
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoiteMedicale(int id, UpdateBoiteMedicaleDto updateBoiteDto)
        {
            var boite = await _context.BoitesMedicales.FindAsync(id);
            if (boite == null)
            {
                return NotFound($"Boîte médicale avec l'ID {id} non trouvée.");
            }

            if (!string.IsNullOrEmpty(updateBoiteDto.Numero) && updateBoiteDto.Numero != boite.Numero)
            {
                if (await _context.BoitesMedicales.AnyAsync(b => b.Numero == updateBoiteDto.Numero && b.Id != id))
                {
                    return BadRequest($"Une boîte médicale avec le numéro '{updateBoiteDto.Numero}' existe déjà.");
                }
                boite.Numero = updateBoiteDto.Numero;
            }

            if (!string.IsNullOrEmpty(updateBoiteDto.Name))
                boite.Name = updateBoiteDto.Name;
            if (updateBoiteDto.Type.HasValue)
                boite.Type = updateBoiteDto.Type.Value;
            if (updateBoiteDto.Status.HasValue)
                boite.Status = updateBoiteDto.Status.Value;
            if (updateBoiteDto.Description != null)
                boite.Description = updateBoiteDto.Description;
            if (updateBoiteDto.DateExpiration.HasValue)
                boite.DateExpiration = updateBoiteDto.DateExpiration.Value;
            if (updateBoiteDto.DerniereMaintenance.HasValue)
                boite.DerniereMaintenance = updateBoiteDto.DerniereMaintenance.Value;
            if (updateBoiteDto.ProchaineMaintenance.HasValue)
                boite.ProchaineMaintenance = updateBoiteDto.ProchaineMaintenance;
            if (updateBoiteDto.ResponsableMaintenance != null)
                boite.ResponsableMaintenance = updateBoiteDto.ResponsableMaintenance;
            if (updateBoiteDto.IsActive.HasValue)
                boite.IsActive = updateBoiteDto.IsActive.Value;

            boite.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoiteMedicaleExists(id))
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
        /// Supprime une boîte médicale (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoiteMedicale(int id)
        {
            var boite = await _context.BoitesMedicales.FindAsync(id);
            if (boite == null)
            {
                return NotFound($"Boîte médicale avec l'ID {id} non trouvée.");
            }

            if (boite.Status == StatusBoiteMedicale.Assignee)
            {
                return BadRequest("Une boîte médicale assignée ne peut pas être supprimée. Veuillez d'abord la désassigner.");
            }

            boite.IsActive = false;
            boite.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Boîte médicale '{boite.Numero}' désactivée avec succès." });
        }

        /// <summary>
        /// Supprime définitivement une boîte médicale et ses items (hard delete)
        /// </summary>
        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> DeleteBoiteMedicalePermanent(int id)
        {
            var boite = await _context.BoitesMedicales
                .Include(b => b.Items)
                .Include(b => b.VolBoitesMedicales)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boite == null)
            {
                return NotFound($"Boîte médicale avec l'ID {id} non trouvée.");
            }

            if (boite.Status == StatusBoiteMedicale.Assignee || boite.VolBoitesMedicales.Any())
            {
                return BadRequest("Une boîte médicale assignée ne peut pas être supprimée. Veuillez d'abord la désassigner.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Supprimer les items
                _context.BoiteMedicaleItems.RemoveRange(boite.Items);
                
                // Supprimer la boîte
                _context.BoitesMedicales.Remove(boite);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Boîte médicale '{boite.Numero}' supprimée définitivement.",
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
        /// Assigne une boîte médicale à un vol
        /// </summary>
        [HttpPost("{boiteId}/assign-to-vol/{volId}")]
        public async Task<ActionResult<VolBoiteMedicaleDto>> AssignBoiteToVol(int boiteId, int volId, CreateVolBoiteMedicaleDto createVolBoiteDto)
        {
            var boite = await _context.BoitesMedicales.FindAsync(boiteId);
            if (boite == null)
            {
                return NotFound($"Boîte médicale avec l'ID {boiteId} non trouvée.");
            }

            var vol = await _context.Vols.FindAsync(volId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {volId} non trouvé.");
            }

            if (boite.Status != StatusBoiteMedicale.Disponible)
            {
                return BadRequest($"La boîte médicale {boite.Numero} n'est pas disponible pour assignation.");
            }

            // Vérifier si cette boîte n'est pas déjà assignée à ce vol
            if (await _context.VolBoitesMedicales.AnyAsync(vbm => vbm.VolId == volId && vbm.BoiteMedicaleId == boiteId))
            {
                return BadRequest("Cette boîte médicale est déjà assignée à ce vol.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var volBoite = new VolBoiteMedicale
                {
                    VolId = volId,
                    BoiteMedicaleId = boiteId,
                    DateAssignation = DateTime.UtcNow,
                    AssignePar = createVolBoiteDto.AssignePar ?? HttpContext.User?.Identity?.Name,
                    Commentaires = createVolBoiteDto.Commentaires
                };

                _context.VolBoitesMedicales.Add(volBoite);

                // Mettre à jour le statut de la boîte
                boite.Status = StatusBoiteMedicale.Assignee;
                boite.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var volBoiteDto = new VolBoiteMedicaleDto
                {
                    Id = volBoite.Id,
                    VolId = volBoite.VolId,
                    BoiteMedicaleId = volBoite.BoiteMedicaleId,
                    DateAssignation = volBoite.DateAssignation,
                    AssignePar = volBoite.AssignePar,
                    Commentaires = volBoite.Commentaires,
                    Vol = new VolDto
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
                    },
                    BoiteMedicale = new BoiteMedicaleDto
                    {
                        Id = boite.Id,
                        Numero = boite.Numero,
                        Name = boite.Name,
                        Type = boite.Type,
                        Status = boite.Status,
                        Description = boite.Description,
                        DateExpiration = boite.DateExpiration,
                        DerniereMaintenance = boite.DerniereMaintenance,
                        ProchaineMaintenance = boite.ProchaineMaintenance,
                        ResponsableMaintenance = boite.ResponsableMaintenance,
                        IsActive = boite.IsActive,
                        CreatedAt = boite.CreatedAt,
                        UpdatedAt = boite.UpdatedAt
                    }
                };

                return Ok(volBoiteDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Désassigne une boîte médicale d'un vol
        /// </summary>
        [HttpDelete("vol-assignments/{volBoiteId}")]
        public async Task<IActionResult> UnassignBoiteFromVol(int volBoiteId)
        {
            var volBoite = await _context.VolBoitesMedicales
                .Include(vbm => vbm.BoiteMedicale)
                .FirstOrDefaultAsync(vbm => vbm.Id == volBoiteId);

            if (volBoite == null)
            {
                return NotFound($"Assignation avec l'ID {volBoiteId} non trouvée.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.VolBoitesMedicales.Remove(volBoite);

                // Remettre la boîte comme disponible
                volBoite.BoiteMedicale.Status = StatusBoiteMedicale.Disponible;
                volBoite.BoiteMedicale.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Boîte médicale désassignée avec succès." });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Désassigne une boîte médicale (par son ID, supprime toutes ses assignations)
        /// </summary>
        [HttpPost("{boiteId}/unassign")]
        public async Task<IActionResult> UnassignBoite(int boiteId)
        {
            var boite = await _context.BoitesMedicales
                .Include(b => b.VolBoitesMedicales)
                .FirstOrDefaultAsync(b => b.Id == boiteId);

            if (boite == null)
            {
                return NotFound($"Boîte médicale avec l'ID {boiteId} non trouvée.");
            }

            if (!boite.VolBoitesMedicales.Any())
            {
                return BadRequest("Cette boîte médicale n'est assignée à aucun vol.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Supprimer toutes les assignations
                _context.VolBoitesMedicales.RemoveRange(boite.VolBoitesMedicales);

                // Remettre la boîte comme disponible
                boite.Status = StatusBoiteMedicale.Disponible;
                boite.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = $"Boîte médicale '{boite.Numero}' désassignée avec succès." });
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return StatusCode(500, $"Erreur lors de la désassignation: {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère les boîtes médicales disponibles
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<BoiteMedicaleDto>>> GetAvailableBoites()
        {
            var boites = await _context.BoitesMedicales
                .Where(b => b.IsActive && b.Status == StatusBoiteMedicale.Disponible && b.DateExpiration > DateTime.UtcNow)
                .Select(b => new BoiteMedicaleDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    Name = b.Name,
                    Type = b.Type,
                    Status = b.Status,
                    Description = b.Description,
                    DateExpiration = b.DateExpiration,
                    DerniereMaintenance = b.DerniereMaintenance,
                    ProchaineMaintenance = b.ProchaineMaintenance,
                    ResponsableMaintenance = b.ResponsableMaintenance,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync();

            return Ok(boites);
        }

        /// <summary>
        /// Récupère les boîtes médicales par type
        /// </summary>
        [HttpGet("by-type/{type}")]
        public async Task<ActionResult<IEnumerable<BoiteMedicaleDto>>> GetBoitesByType(TypeBoiteMedicale type)
        {
            var boites = await _context.BoitesMedicales
                .Where(b => b.Type == type && b.IsActive)
                .Select(b => new BoiteMedicaleDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    Name = b.Name,
                    Type = b.Type,
                    Status = b.Status,
                    Description = b.Description,
                    DateExpiration = b.DateExpiration,
                    DerniereMaintenance = b.DerniereMaintenance,
                    ProchaineMaintenance = b.ProchaineMaintenance,
                    ResponsableMaintenance = b.ResponsableMaintenance,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync();

            return Ok(boites);
        }

        /// <summary>
        /// Récupère les boîtes médicales expirées ou proche de l'expiration
        /// </summary>
        [HttpGet("expiring")]
        public async Task<ActionResult<IEnumerable<BoiteMedicaleDto>>> GetExpiringBoites([FromQuery] int daysBeforeExpiration = 30)
        {
            var dateLimit = DateTime.UtcNow.AddDays(daysBeforeExpiration);

            var boites = await _context.BoitesMedicales
                .Where(b => b.IsActive && b.DateExpiration <= dateLimit)
                .Select(b => new BoiteMedicaleDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    Name = b.Name,
                    Type = b.Type,
                    Status = b.Status,
                    Description = b.Description,
                    DateExpiration = b.DateExpiration,
                    DerniereMaintenance = b.DerniereMaintenance,
                    ProchaineMaintenance = b.ProchaineMaintenance,
                    ResponsableMaintenance = b.ResponsableMaintenance,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .OrderBy(b => b.DateExpiration)
                .ToListAsync();

            return Ok(boites);
        }

        /// <summary>
        /// Recherche des boîtes médicales par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BoiteMedicaleDto>>> SearchBoites(
            [FromQuery] string? numero,
            [FromQuery] string? name,
            [FromQuery] TypeBoiteMedicale? type,
            [FromQuery] StatusBoiteMedicale? status,
            [FromQuery] bool activeOnly = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.BoitesMedicales.AsQueryable();

            if (activeOnly)
                query = query.Where(b => b.IsActive);

            if (!string.IsNullOrEmpty(numero))
                query = query.Where(b => b.Numero.Contains(numero));

            if (!string.IsNullOrEmpty(name))
                query = query.Where(b => b.Name.Contains(name));

            if (type.HasValue)
                query = query.Where(b => b.Type == type.Value);

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            var boites = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BoiteMedicaleDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    Name = b.Name,
                    Type = b.Type,
                    Status = b.Status,
                    Description = b.Description,
                    DateExpiration = b.DateExpiration,
                    DerniereMaintenance = b.DerniereMaintenance,
                    ProchaineMaintenance = b.ProchaineMaintenance,
                    ResponsableMaintenance = b.ResponsableMaintenance,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync();

            return Ok(boites);
        }

        private bool BoiteMedicaleExists(int id)
        {
            return _context.BoitesMedicales.Any(e => e.Id == id);
        }
    }
}
