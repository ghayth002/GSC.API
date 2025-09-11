using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DossiersVolController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DossiersVolController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les dossiers de vol avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DossierVolDto>>> GetDossiersVol([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var dossiers = await _context.DossiersVol
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DossierVolDto
                {
                    Id = d.Id,
                    VolId = d.VolId,
                    Numero = d.Numero,
                    Status = d.Status,
                    DateCreation = d.DateCreation,
                    DateValidation = d.DateValidation,
                    ValidePar = d.ValidePar,
                    Resume = d.Resume,
                    Commentaires = d.Commentaires,
                    CoutTotal = d.CoutTotal,
                    NombreEcarts = d.NombreEcarts,
                    MontantEcarts = d.MontantEcarts,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync();

            return Ok(dossiers);
        }

        /// <summary>
        /// Récupère un dossier de vol par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DossierVolDetailsDto>> GetDossierVol(int id)
        {
            var dossier = await _context.DossiersVol
                .Include(d => d.Vol)
                .Include(d => d.Documents)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dossier == null)
            {
                return NotFound($"Dossier de vol avec l'ID {id} non trouvé.");
            }

            var dossierDto = new DossierVolDetailsDto
            {
                Id = dossier.Id,
                VolId = dossier.VolId,
                Numero = dossier.Numero,
                Status = dossier.Status,
                DateCreation = dossier.DateCreation,
                DateValidation = dossier.DateValidation,
                ValidePar = dossier.ValidePar,
                Resume = dossier.Resume,
                Commentaires = dossier.Commentaires,
                CoutTotal = dossier.CoutTotal,
                NombreEcarts = dossier.NombreEcarts,
                MontantEcarts = dossier.MontantEcarts,
                CreatedAt = dossier.CreatedAt,
                UpdatedAt = dossier.UpdatedAt,
                Vol = new VolDto
                {
                    Id = dossier.Vol.Id,
                    FlightNumber = dossier.Vol.FlightNumber,
                    FlightDate = dossier.Vol.FlightDate,
                    DepartureTime = dossier.Vol.DepartureTime,
                    ArrivalTime = dossier.Vol.ArrivalTime,
                    Aircraft = dossier.Vol.Aircraft,
                    Origin = dossier.Vol.Origin,
                    Destination = dossier.Vol.Destination,
                    Zone = dossier.Vol.Zone,
                    EstimatedPassengers = dossier.Vol.EstimatedPassengers,
                    ActualPassengers = dossier.Vol.ActualPassengers,
                    Duration = dossier.Vol.Duration,
                    Season = dossier.Vol.Season,
                    CreatedAt = dossier.Vol.CreatedAt,
                    UpdatedAt = dossier.Vol.UpdatedAt
                },
                Documents = dossier.Documents.Select(doc => new DossierVolDocumentDto
                {
                    Id = doc.Id,
                    DossierVolId = doc.DossierVolId,
                    NomDocument = doc.NomDocument,
                    TypeDocument = doc.TypeDocument,
                    CheminFichier = doc.CheminFichier,
                    FormatFichier = doc.FormatFichier,
                    TailleFichier = doc.TailleFichier,
                    DateAjout = doc.DateAjout,
                    AjoutePar = doc.AjoutePar
                }).ToList()
            };

            return Ok(dossierDto);
        }

        /// <summary>
        /// Crée un nouveau dossier de vol
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DossierVolDto>> CreateDossierVol(CreateDossierVolDto createDossierDto)
        {
            // Vérifier que le vol existe
            var vol = await _context.Vols.FindAsync(createDossierDto.VolId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {createDossierDto.VolId} non trouvé.");
            }

            // Vérifier qu'il n'y a pas déjà un dossier pour ce vol
            if (await _context.DossiersVol.AnyAsync(d => d.VolId == createDossierDto.VolId))
            {
                return BadRequest($"Un dossier de vol existe déjà pour le vol {createDossierDto.VolId}.");
            }

            // Vérifier l'unicité du numéro
            if (await _context.DossiersVol.AnyAsync(d => d.Numero == createDossierDto.Numero))
            {
                return BadRequest($"Un dossier avec le numéro '{createDossierDto.Numero}' existe déjà.");
            }

            var dossier = new DossierVol
            {
                VolId = createDossierDto.VolId,
                Numero = createDossierDto.Numero,
                Status = StatusDossierVol.EnPreparation,
                DateCreation = DateTime.UtcNow,
                Resume = createDossierDto.Resume,
                Commentaires = createDossierDto.Commentaires,
                CoutTotal = 0,
                NombreEcarts = 0,
                MontantEcarts = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.DossiersVol.Add(dossier);
            await _context.SaveChangesAsync();

            var dossierDto = new DossierVolDto
            {
                Id = dossier.Id,
                VolId = dossier.VolId,
                Numero = dossier.Numero,
                Status = dossier.Status,
                DateCreation = dossier.DateCreation,
                DateValidation = dossier.DateValidation,
                ValidePar = dossier.ValidePar,
                Resume = dossier.Resume,
                Commentaires = dossier.Commentaires,
                CoutTotal = dossier.CoutTotal,
                NombreEcarts = dossier.NombreEcarts,
                MontantEcarts = dossier.MontantEcarts,
                CreatedAt = dossier.CreatedAt,
                UpdatedAt = dossier.UpdatedAt
            };

            return CreatedAtAction(nameof(GetDossierVol), new { id = dossier.Id }, dossierDto);
        }

        /// <summary>
        /// Met à jour un dossier de vol existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDossierVol(int id, UpdateDossierVolDto updateDossierDto)
        {
            var dossier = await _context.DossiersVol.FindAsync(id);
            if (dossier == null)
            {
                return NotFound($"Dossier de vol avec l'ID {id} non trouvé.");
            }

            if (!string.IsNullOrEmpty(updateDossierDto.Numero) && updateDossierDto.Numero != dossier.Numero)
            {
                if (await _context.DossiersVol.AnyAsync(d => d.Numero == updateDossierDto.Numero && d.Id != id))
                {
                    return BadRequest($"Un dossier avec le numéro '{updateDossierDto.Numero}' existe déjà.");
                }
                dossier.Numero = updateDossierDto.Numero;
            }

            if (updateDossierDto.Status.HasValue)
            {
                dossier.Status = updateDossierDto.Status.Value;
                if (updateDossierDto.Status.Value == StatusDossierVol.Valide && dossier.DateValidation == null)
                {
                    dossier.DateValidation = updateDossierDto.DateValidation ?? DateTime.UtcNow;
                    dossier.ValidePar = updateDossierDto.ValidePar ?? HttpContext.User?.Identity?.Name;
                }
            }

            if (updateDossierDto.Resume != null)
                dossier.Resume = updateDossierDto.Resume;
            if (updateDossierDto.Commentaires != null)
                dossier.Commentaires = updateDossierDto.Commentaires;

            dossier.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DossierVolExists(id))
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
        /// Supprime un dossier de vol
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDossierVol(int id)
        {
            var dossier = await _context.DossiersVol
                .Include(d => d.Documents)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dossier == null)
            {
                return NotFound($"Dossier de vol avec l'ID {id} non trouvé.");
            }

            if (dossier.Status == StatusDossierVol.Valide || dossier.Status == StatusDossierVol.Archive)
            {
                return BadRequest("Un dossier validé ou archivé ne peut pas être supprimé.");
            }

            _context.DossiersVol.Remove(dossier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Génère automatiquement un dossier de vol complet à partir des données du vol
        /// </summary>
        [HttpPost("generate-from-vol/{volId}")]
        public async Task<ActionResult<DossierVolDto>> GenerateDossierFromVol(int volId)
        {
            var vol = await _context.Vols
                .Include(v => v.BonsCommandePrevisionnels)
                    .ThenInclude(bcp => bcp.Lignes)
                .Include(v => v.BonsLivraison)
                    .ThenInclude(bl => bl.Lignes)
                .Include(v => v.VolBoitesMedicales)
                    .ThenInclude(vbm => vbm.BoiteMedicale)
                .FirstOrDefaultAsync(v => v.Id == volId);

            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {volId} non trouvé.");
            }

            // Vérifier qu'il n'y a pas déjà un dossier pour ce vol
            if (await _context.DossiersVol.AnyAsync(d => d.VolId == volId))
            {
                return BadRequest($"Un dossier de vol existe déjà pour le vol {volId}.");
            }

            // Calculer les statistiques du vol
            var coutTotal = vol.BonsLivraison.Where(bl => bl.Status == StatusBL.Valide).Sum(bl => bl.MontantTotal);
            var ecarts = await _context.Ecarts.Where(e => e.VolId == volId).ToListAsync();
            var nombreEcarts = ecarts.Count;
            var montantEcarts = ecarts.Sum(e => Math.Abs(e.EcartMontant));

            // Générer un numéro unique pour le dossier
            var numero = $"DV-{vol.FlightNumber}-{vol.FlightDate:yyyyMMdd}";

            // Générer le résumé automatiquement
            var resume = GenerateVolSummary(vol, ecarts);

            var createDossierDto = new CreateDossierVolDto
            {
                VolId = volId,
                Numero = numero,
                Resume = resume,
                Commentaires = $"Dossier généré automatiquement le {DateTime.Now:dd/MM/yyyy à HH:mm}"
            };

            var result = await CreateDossierVol(createDossierDto);

            if (result.Result is CreatedAtActionResult createdResult && createdResult.Value is DossierVolDto dossierDto)
            {
                // Mettre à jour les statistiques calculées
                var dossier = await _context.DossiersVol.FindAsync(dossierDto.Id);
                if (dossier != null)
                {
                    dossier.CoutTotal = coutTotal;
                    dossier.NombreEcarts = nombreEcarts;
                    dossier.MontantEcarts = montantEcarts;
                    dossier.Status = StatusDossierVol.Complete;
                    await _context.SaveChangesAsync();

                    dossierDto.CoutTotal = coutTotal;
                    dossierDto.NombreEcarts = nombreEcarts;
                    dossierDto.MontantEcarts = montantEcarts;
                    dossierDto.Status = StatusDossierVol.Complete;
                }
            }

            return result;
        }

        /// <summary>
        /// Valide un dossier de vol
        /// </summary>
        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateDossierVol(int id, [FromBody] string? commentaires = null)
        {
            var dossier = await _context.DossiersVol.FindAsync(id);
            if (dossier == null)
            {
                return NotFound($"Dossier de vol avec l'ID {id} non trouvé.");
            }

            if (dossier.Status == StatusDossierVol.Valide)
            {
                return BadRequest("Ce dossier est déjà validé.");
            }

            if (dossier.Status != StatusDossierVol.Complete)
            {
                return BadRequest("Seuls les dossiers complets peuvent être validés.");
            }

            dossier.Status = StatusDossierVol.Valide;
            dossier.DateValidation = DateTime.UtcNow;
            dossier.ValidePar = HttpContext.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(commentaires))
            {
                dossier.Commentaires = string.IsNullOrEmpty(dossier.Commentaires) 
                    ? commentaires 
                    : $"{dossier.Commentaires}\n\nValidation: {commentaires}";
            }
            dossier.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Dossier validé avec succès" });
        }

        /// <summary>
        /// Archive un dossier de vol
        /// </summary>
        [HttpPost("{id}/archive")]
        public async Task<IActionResult> ArchiveDossierVol(int id)
        {
            var dossier = await _context.DossiersVol.FindAsync(id);
            if (dossier == null)
            {
                return NotFound($"Dossier de vol avec l'ID {id} non trouvé.");
            }

            if (dossier.Status != StatusDossierVol.Valide)
            {
                return BadRequest("Seuls les dossiers validés peuvent être archivés.");
            }

            dossier.Status = StatusDossierVol.Archive;
            dossier.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Dossier archivé avec succès" });
        }

        /// <summary>
        /// Ajoute un document à un dossier de vol
        /// </summary>
        [HttpPost("{dossierId}/documents")]
        public async Task<ActionResult<DossierVolDocumentDto>> AddDocumentToDossier(int dossierId, CreateDossierVolDocumentDto createDocumentDto)
        {
            var dossier = await _context.DossiersVol.FindAsync(dossierId);
            if (dossier == null)
            {
                return NotFound($"Dossier de vol avec l'ID {dossierId} non trouvé.");
            }

            var document = new DossierVolDocument
            {
                DossierVolId = dossierId,
                NomDocument = createDocumentDto.NomDocument,
                TypeDocument = createDocumentDto.TypeDocument,
                CheminFichier = createDocumentDto.CheminFichier,
                FormatFichier = createDocumentDto.FormatFichier,
                TailleFichier = createDocumentDto.TailleFichier,
                DateAjout = DateTime.UtcNow,
                AjoutePar = createDocumentDto.AjoutePar ?? HttpContext.User?.Identity?.Name
            };

            _context.DossierVolDocuments.Add(document);
            await _context.SaveChangesAsync();

            var documentDto = new DossierVolDocumentDto
            {
                Id = document.Id,
                DossierVolId = document.DossierVolId,
                NomDocument = document.NomDocument,
                TypeDocument = document.TypeDocument,
                CheminFichier = document.CheminFichier,
                FormatFichier = document.FormatFichier,
                TailleFichier = document.TailleFichier,
                DateAjout = document.DateAjout,
                AjoutePar = document.AjoutePar
            };

            return Ok(documentDto);
        }

        /// <summary>
        /// Supprime un document d'un dossier de vol
        /// </summary>
        [HttpDelete("documents/{documentId}")]
        public async Task<IActionResult> DeleteDocumentFromDossier(int documentId)
        {
            var document = await _context.DossierVolDocuments.FindAsync(documentId);
            if (document == null)
            {
                return NotFound($"Document avec l'ID {documentId} non trouvé.");
            }

            _context.DossierVolDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Récupère le dossier de vol d'un vol spécifique
        /// </summary>
        [HttpGet("by-vol/{volId}")]
        public async Task<ActionResult<DossierVolDetailsDto>> GetDossierByVol(int volId)
        {
            var dossier = await _context.DossiersVol
                .Include(d => d.Vol)
                .Include(d => d.Documents)
                .FirstOrDefaultAsync(d => d.VolId == volId);

            if (dossier == null)
            {
                return NotFound($"Aucun dossier de vol trouvé pour le vol {volId}.");
            }

            return await GetDossierVol(dossier.Id);
        }

        /// <summary>
        /// Recherche des dossiers de vol par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DossierVolDto>>> SearchDossiers(
            [FromQuery] string? numero,
            [FromQuery] int? volId,
            [FromQuery] StatusDossierVol? status,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? validePar,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.DossiersVol.AsQueryable();

            if (!string.IsNullOrEmpty(numero))
                query = query.Where(d => d.Numero.Contains(numero));

            if (volId.HasValue)
                query = query.Where(d => d.VolId == volId.Value);

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            if (dateDebut.HasValue)
                query = query.Where(d => d.DateCreation >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(d => d.DateCreation <= dateFin.Value);

            if (!string.IsNullOrEmpty(validePar))
                query = query.Where(d => d.ValidePar != null && d.ValidePar.Contains(validePar));

            var dossiers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DossierVolDto
                {
                    Id = d.Id,
                    VolId = d.VolId,
                    Numero = d.Numero,
                    Status = d.Status,
                    DateCreation = d.DateCreation,
                    DateValidation = d.DateValidation,
                    ValidePar = d.ValidePar,
                    Resume = d.Resume,
                    Commentaires = d.Commentaires,
                    CoutTotal = d.CoutTotal,
                    NombreEcarts = d.NombreEcarts,
                    MontantEcarts = d.MontantEcarts,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync();

            return Ok(dossiers);
        }

        /// <summary>
        /// Génère un résumé automatique du vol
        /// </summary>
        private string GenerateVolSummary(Vol vol, List<Ecart> ecarts)
        {
            var summary = $"DOSSIER DE VOL - {vol.FlightNumber}\n";
            summary += $"Date: {vol.FlightDate:dd/MM/yyyy}\n";
            summary += $"Route: {vol.Origin} → {vol.Destination}\n";
            summary += $"Avion: {vol.Aircraft}\n";
            summary += $"Passagers prévus: {vol.EstimatedPassengers}, Réels: {vol.ActualPassengers}\n\n";

            summary += "COMMANDES ET LIVRAISONS:\n";
            summary += $"- Nombre de BCP: {vol.BonsCommandePrevisionnels.Count}\n";
            summary += $"- Nombre de BL: {vol.BonsLivraison.Count}\n";
            summary += $"- BL validés: {vol.BonsLivraison.Count(bl => bl.Status == StatusBL.Valide)}\n\n";

            if (ecarts.Any())
            {
                summary += "ÉCARTS DÉTECTÉS:\n";
                summary += $"- Nombre total: {ecarts.Count}\n";
                summary += $"- En attente: {ecarts.Count(e => e.Status == StatusEcart.EnAttente)}\n";
                summary += $"- Résolus: {ecarts.Count(e => e.Status == StatusEcart.Resolu)}\n";
                summary += $"- Montant total des écarts: {ecarts.Sum(e => Math.Abs(e.EcartMontant)):C}\n\n";
            }

            summary += "ÉQUIPEMENTS MÉDICAUX:\n";
            summary += $"- Boîtes médicales assignées: {vol.VolBoitesMedicales.Count}\n";

            return summary;
        }

        private bool DossierVolExists(int id)
        {
            return _context.DossiersVol.Any(e => e.Id == id);
        }
    }
}
