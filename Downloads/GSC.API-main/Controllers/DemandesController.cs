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
    [Authorize]
    public class DemandesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DemandesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère toutes les demandes de menu avec pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator,Manager,Fournisseur")]
        public async Task<ActionResult<IEnumerable<DemandeMenuDto>>> GetDemandes(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] StatusDemande? status = null,
            [FromQuery] TypeDemande? type = null)
        {
            var query = _context.DemandesMenu
                .Include(d => d.DemandeParUser)
                .Include(d => d.AssigneAFournisseur)
                .Include(d => d.DemandePlats)
                    .ThenInclude(dp => dp.ArticleProposed)
                .Include(d => d.Reponses)
                    .ThenInclude(r => r.MenuProposed)
                .Where(d => d.IsActive)
                .AsQueryable();

            // Filtrer pour les fournisseurs
            if (User.IsInRole("Fournisseur"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                query = query.Where(d => d.AssigneAFournisseurId == userId);
            }

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            if (type.HasValue)
                query = query.Where(d => d.Type == type.Value);

            var demandes = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DemandeMenuDto
                {
                    Id = d.Id,
                    Numero = d.Numero,
                    Titre = d.Titre,
                    Description = d.Description,
                    Type = d.Type,
                    Status = d.Status,
                    DateDemande = d.DateDemande,
                    DateLimite = d.DateLimite,
                    DateReponse = d.DateReponse,
                    DemandeParUserId = d.DemandeParUserId,
                    DemandeParUserName = $"{d.DemandeParUser.FirstName} {d.DemandeParUser.LastName}",
                    AssigneAFournisseurId = d.AssigneAFournisseurId,
                    AssigneAFournisseurName = d.AssigneAFournisseur != null ? 
                        $"{d.AssigneAFournisseur.FirstName} {d.AssigneAFournisseur.LastName}" : null,
                    CommentairesAdmin = d.CommentairesAdmin,
                    CommentairesFournisseur = d.CommentairesFournisseur,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    DemandePlats = d.DemandePlats.Select(dp => new DemandePlatDto
                    {
                        Id = dp.Id,
                        DemandeMenuId = dp.DemandeMenuId,
                        NomPlatSouhaite = dp.NomPlatSouhaite,
                        DescriptionSouhaitee = dp.DescriptionSouhaitee,
                        TypePlat = dp.TypePlat,
                        UniteSouhaitee = dp.UniteSouhaitee,
                        PrixMaximal = dp.PrixMaximal,
                        QuantiteEstimee = dp.QuantiteEstimee,
                        SpecificationsSpeciales = dp.SpecificationsSpeciales,
                        IsObligatoire = dp.IsObligatoire,
                        Status = dp.Status,
                        ArticleProposedId = dp.ArticleProposedId,
                        CommentairesFournisseur = dp.CommentairesFournisseur,
                        CreatedAt = dp.CreatedAt,
                        UpdatedAt = dp.UpdatedAt
                    }).ToList(),
                    Reponses = d.Reponses.Select(r => new DemandeMenuReponseDto
                    {
                        Id = r.Id,
                        DemandeMenuId = r.DemandeMenuId,
                        MenuProposedId = r.MenuProposedId,
                        NomMenuPropose = r.NomMenuPropose,
                        DescriptionMenuPropose = r.DescriptionMenuPropose,
                        PrixTotal = r.PrixTotal,
                        CommentairesFournisseur = r.CommentairesFournisseur,
                        IsAcceptedByAdmin = r.IsAcceptedByAdmin,
                        DateProposition = r.DateProposition,
                        DateAcceptation = r.DateAcceptation,
                        CommentairesAcceptation = r.CommentairesAcceptation
                    }).ToList()
                })
                .ToListAsync();

            return Ok(demandes);
        }

        /// <summary>
        /// Récupère une demande de menu par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,Manager,Fournisseur")]
        public async Task<ActionResult<DemandeMenuDto>> GetDemande(int id)
        {
            var demande = await _context.DemandesMenu
                .Include(d => d.DemandeParUser)
                .Include(d => d.AssigneAFournisseur)
                .Include(d => d.DemandePlats)
                    .ThenInclude(dp => dp.ArticleProposed)
                .Include(d => d.Reponses)
                    .ThenInclude(r => r.MenuProposed)
                        .ThenInclude(m => m.MenuItems)
                            .ThenInclude(mi => mi.Article)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {id} non trouvée.");
            }

            // Vérification accès fournisseur
            if (User.IsInRole("Fournisseur"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (demande.AssigneAFournisseurId != userId)
                {
                    return Forbid();
                }
            }

            var demandeDto = new DemandeMenuDto
            {
                Id = demande.Id,
                Numero = demande.Numero,
                Titre = demande.Titre,
                Description = demande.Description,
                Type = demande.Type,
                Status = demande.Status,
                DateDemande = demande.DateDemande,
                DateLimite = demande.DateLimite,
                DateReponse = demande.DateReponse,
                DemandeParUserId = demande.DemandeParUserId,
                DemandeParUserName = $"{demande.DemandeParUser.FirstName} {demande.DemandeParUser.LastName}",
                AssigneAFournisseurId = demande.AssigneAFournisseurId,
                AssigneAFournisseurName = demande.AssigneAFournisseur != null ? 
                    $"{demande.AssigneAFournisseur.FirstName} {demande.AssigneAFournisseur.LastName}" : null,
                CommentairesAdmin = demande.CommentairesAdmin,
                CommentairesFournisseur = demande.CommentairesFournisseur,
                IsActive = demande.IsActive,
                CreatedAt = demande.CreatedAt,
                UpdatedAt = demande.UpdatedAt,
                DemandePlats = demande.DemandePlats.Select(dp => new DemandePlatDto
                {
                    Id = dp.Id,
                    DemandeMenuId = dp.DemandeMenuId,
                    NomPlatSouhaite = dp.NomPlatSouhaite,
                    DescriptionSouhaitee = dp.DescriptionSouhaitee,
                    TypePlat = dp.TypePlat,
                    UniteSouhaitee = dp.UniteSouhaitee,
                    PrixMaximal = dp.PrixMaximal,
                    QuantiteEstimee = dp.QuantiteEstimee,
                    SpecificationsSpeciales = dp.SpecificationsSpeciales,
                    IsObligatoire = dp.IsObligatoire,
                    Status = dp.Status,
                    ArticleProposedId = dp.ArticleProposedId,
                    ArticleProposed = dp.ArticleProposed != null ? new ArticleDto
                    {
                        Id = dp.ArticleProposed.Id,
                        Code = dp.ArticleProposed.Code,
                        Name = dp.ArticleProposed.Name,
                        Description = dp.ArticleProposed.Description,
                        Type = dp.ArticleProposed.Type,
                        Unit = dp.ArticleProposed.Unit,
                        UnitPrice = dp.ArticleProposed.UnitPrice,
                        Supplier = dp.ArticleProposed.Supplier,
                        IsActive = dp.ArticleProposed.IsActive,
                        CreatedAt = dp.ArticleProposed.CreatedAt,
                        UpdatedAt = dp.ArticleProposed.UpdatedAt
                    } : null,
                    CommentairesFournisseur = dp.CommentairesFournisseur,
                    CreatedAt = dp.CreatedAt,
                    UpdatedAt = dp.UpdatedAt
                }).ToList(),
                Reponses = demande.Reponses.Select(r => new DemandeMenuReponseDto
                {
                    Id = r.Id,
                    DemandeMenuId = r.DemandeMenuId,
                    MenuProposedId = r.MenuProposedId,
                    MenuProposed = new MenuDto
                    {
                        Id = r.MenuProposed.Id,
                        Name = r.MenuProposed.Name,
                        Description = r.MenuProposed.Description,
                        TypePassager = r.MenuProposed.TypePassager,
                        Season = r.MenuProposed.Season,
                        Zone = r.MenuProposed.Zone,
                        IsActive = r.MenuProposed.IsActive,
                        CreatedAt = r.MenuProposed.CreatedAt,
                        UpdatedAt = r.MenuProposed.UpdatedAt
                    },
                    NomMenuPropose = r.NomMenuPropose,
                    DescriptionMenuPropose = r.DescriptionMenuPropose,
                    PrixTotal = r.PrixTotal,
                    CommentairesFournisseur = r.CommentairesFournisseur,
                    IsAcceptedByAdmin = r.IsAcceptedByAdmin,
                    DateProposition = r.DateProposition,
                    DateAcceptation = r.DateAcceptation,
                    CommentairesAcceptation = r.CommentairesAcceptation
                }).ToList()
            };

            return Ok(demandeDto);
        }

        /// <summary>
        /// Refuse une demande de menu (Fournisseur uniquement)
        /// </summary>
        [HttpPost("{id}/refuse")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<IActionResult> RefuseDemande(int id, RefuseDemandeDto refuseDto)
        {
            var demande = await _context.DemandesMenu.FindAsync(id);
            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {id} non trouvée.");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (demande.AssigneAFournisseurId != userId)
            {
                return Forbid();
            }

            if (demande.Status == StatusDemande.Acceptee || demande.Status == StatusDemande.Refusee || demande.Status == StatusDemande.Completee)
            {
                return BadRequest("Cette demande a déjà été traitée.");
            }

            demande.Status = StatusDemande.Refusee;
            demande.CommentairesFournisseur = refuseDto.Raison;
            demande.DateReponse = DateTime.UtcNow;
            demande.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Demande refusée avec succès." });
        }

        /// <summary>
        /// Crée une nouvelle demande de menu (Admin uniquement)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<DemandeMenuDto>> CreateDemande(CreateDemandeMenuDto createDemandeDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Vérifier le fournisseur si assigné
            int? assigneAFournisseurUserId = null;
            StatusDemande initialStatus = StatusDemande.EnAttente;

            if (createDemandeDto.AssigneAFournisseurId.HasValue)
            {
                var fournisseur = await _context.Fournisseurs
                    .FirstOrDefaultAsync(f => f.Id == createDemandeDto.AssigneAFournisseurId.Value);
                
                if (fournisseur == null)
                {
                    return BadRequest($"Fournisseur avec l'ID {createDemandeDto.AssigneAFournisseurId} non trouvé.");
                }

                if (!fournisseur.IsActive)
                {
                    return BadRequest("Le fournisseur sélectionné n'est pas actif.");
                }

                assigneAFournisseurUserId = fournisseur.UserId;
                initialStatus = StatusDemande.EnCours;
            }

            // Générer un numéro unique pour la demande
            var numero = await GenerateDemandeNumero();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var demande = new DemandeMenu
                {
                    Numero = numero,
                    Titre = createDemandeDto.Titre,
                    Description = createDemandeDto.Description,
                    Type = createDemandeDto.Type,
                    Status = initialStatus,
                    DateDemande = DateTime.UtcNow,
                    DateLimite = createDemandeDto.DateLimite,
                    DemandeParUserId = currentUserId,
                    AssigneAFournisseurId = assigneAFournisseurUserId,
                    CommentairesAdmin = createDemandeDto.CommentairesAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DemandesMenu.Add(demande);
                await _context.SaveChangesAsync();

                // Ajouter les plats demandés
                foreach (var platDto in createDemandeDto.DemandePlats)
                {
                    var demandePlat = new DemandePlat
                    {
                        DemandeMenuId = demande.Id,
                        NomPlatSouhaite = platDto.NomPlatSouhaite,
                        DescriptionSouhaitee = platDto.DescriptionSouhaitee,
                        TypePlat = platDto.TypePlat,
                        UniteSouhaitee = platDto.UniteSouhaitee,
                        PrixMaximal = platDto.PrixMaximal,
                        QuantiteEstimee = platDto.QuantiteEstimee,
                        SpecificationsSpeciales = platDto.SpecificationsSpeciales,
                        IsObligatoire = platDto.IsObligatoire,
                        Status = initialStatus,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.DemandePlats.Add(demandePlat);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetDemande), new { id = demande.Id }, 
                    await GetDemandeDto(demande.Id));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Assigne une demande à un fournisseur (Admin uniquement)
        /// </summary>
        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> AssignDemandeToFournisseur(int id, AssignDemandeToFournisseurDto assignDto)
        {
            var demande = await _context.DemandesMenu.FindAsync(id);
            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {id} non trouvée.");
            }

            var fournisseur = await _context.Fournisseurs
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == assignDto.FournisseurId);
            if (fournisseur == null)
            {
                return NotFound($"Fournisseur avec l'ID {assignDto.FournisseurId} non trouvé.");
            }

            if (!fournisseur.IsActive || !fournisseur.IsVerified)
            {
                return BadRequest("Le fournisseur doit être actif et vérifié pour recevoir des demandes.");
            }

            demande.AssigneAFournisseurId = fournisseur.UserId;
            demande.Status = StatusDemande.EnCours;
            demande.CommentairesAdmin = assignDto.CommentairesAdmin;
            demande.DateLimite = assignDto.DateLimite ?? demande.DateLimite;
            demande.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Demande assignée au fournisseur {fournisseur.CompanyName}" });
        }

        /// <summary>
        /// Accepte une réponse de menu d'un fournisseur (Admin uniquement)
        /// </summary>
        [HttpPost("reponses/{reponseId}/accept")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> AcceptMenuReponse(int reponseId, [FromBody] string? commentaires = null)
        {
            var reponse = await _context.DemandeMenuReponses
                .Include(r => r.DemandeMenu)
                .Include(r => r.MenuProposed)
                .FirstOrDefaultAsync(r => r.Id == reponseId);

            if (reponse == null)
            {
                return NotFound($"Réponse avec l'ID {reponseId} non trouvée.");
            }

            if (reponse.IsAcceptedByAdmin)
            {
                return BadRequest("Cette réponse a déjà été acceptée.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Accepter la réponse
                reponse.IsAcceptedByAdmin = true;
                reponse.DateAcceptation = DateTime.UtcNow;
                reponse.CommentairesAcceptation = commentaires;

                // Marquer la demande comme complétée
                reponse.DemandeMenu.Status = StatusDemande.Completee;
                reponse.DemandeMenu.DateReponse = DateTime.UtcNow;
                reponse.DemandeMenu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Réponse acceptée et demande marquée comme complétée" });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Met à jour une demande de menu (Admin uniquement)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> UpdateDemande(int id, UpdateDemandeMenuDto updateDto)
        {
            var demande = await _context.DemandesMenu.FindAsync(id);
            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {id} non trouvée.");
            }

            if (demande.Status == StatusDemande.Completee || demande.Status == StatusDemande.Annulee)
            {
                return BadRequest("Impossible de modifier une demande complétée ou annulée.");
            }

            if (updateDto.Titre != null) demande.Titre = updateDto.Titre;
            if (updateDto.Description != null) demande.Description = updateDto.Description;
            if (updateDto.Type.HasValue) demande.Type = updateDto.Type.Value;
            if (updateDto.DateLimite.HasValue) demande.DateLimite = updateDto.DateLimite.Value;
            if (updateDto.CommentairesAdmin != null) demande.CommentairesAdmin = updateDto.CommentairesAdmin;
            
            // Gestion de l'assignation fournisseur
            if (updateDto.AssigneAFournisseurId.HasValue)
            {
                var fournisseur = await _context.Fournisseurs.FindAsync(updateDto.AssigneAFournisseurId.Value);
                if (fournisseur == null)
                {
                    return BadRequest("Fournisseur introuvable.");
                }
                demande.AssigneAFournisseurId = fournisseur.UserId;
                if (demande.Status == StatusDemande.EnAttente)
                {
                    demande.Status = StatusDemande.EnCours;
                }
            }

            demande.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(await GetDemandeDto(id));
        }

        /// <summary>
        /// Supprime (soft delete) une demande de menu (Admin uniquement)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeleteDemande(int id)
        {
            var demande = await _context.DemandesMenu.FindAsync(id);
            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {id} non trouvée.");
            }

            // Soft delete
            demande.IsActive = false;
            demande.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Demande supprimée avec succès." });
        }

        /// <summary>
        /// Annule une demande (Admin uniquement)
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> CancelDemande(int id, [FromBody] string? raison = null)
        {
            var demande = await _context.DemandesMenu.FindAsync(id);
            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {id} non trouvée.");
            }

            if (demande.Status == StatusDemande.Completee)
            {
                return BadRequest("Une demande complétée ne peut pas être annulée.");
            }

            demande.Status = StatusDemande.Annulee;
            demande.CommentairesAdmin = raison;
            demande.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Demande annulée" });
        }

        private async Task<string> GenerateDemandeNumero()
        {
            var today = DateTime.UtcNow;
            var prefix = $"DEM{today:yyyyMMdd}";
            
            var lastDemande = await _context.DemandesMenu
                .Where(d => d.Numero.StartsWith(prefix))
                .OrderByDescending(d => d.Numero)
                .FirstOrDefaultAsync();

            if (lastDemande == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = lastDemande.Numero.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var number))
            {
                return $"{prefix}{(number + 1):D3}";
            }

            return $"{prefix}001";
        }

        private async Task<DemandeMenuDto> GetDemandeDto(int id)
        {
            var demande = await _context.DemandesMenu
                .Include(d => d.DemandeParUser)
                .Include(d => d.AssigneAFournisseur)
                .Include(d => d.DemandePlats)
                .Include(d => d.Reponses)
                .FirstOrDefaultAsync(d => d.Id == id);

            return new DemandeMenuDto
            {
                Id = demande!.Id,
                Numero = demande.Numero,
                Titre = demande.Titre,
                Description = demande.Description,
                Type = demande.Type,
                Status = demande.Status,
                DateDemande = demande.DateDemande,
                DateLimite = demande.DateLimite,
                DateReponse = demande.DateReponse,
                DemandeParUserId = demande.DemandeParUserId,
                DemandeParUserName = $"{demande.DemandeParUser.FirstName} {demande.DemandeParUser.LastName}",
                AssigneAFournisseurId = demande.AssigneAFournisseurId,
                AssigneAFournisseurName = demande.AssigneAFournisseur != null ? 
                    $"{demande.AssigneAFournisseur.FirstName} {demande.AssigneAFournisseur.LastName}" : null,
                CommentairesAdmin = demande.CommentairesAdmin,
                CommentairesFournisseur = demande.CommentairesFournisseur,
                IsActive = demande.IsActive,
                CreatedAt = demande.CreatedAt,
                UpdatedAt = demande.UpdatedAt,
                DemandePlats = demande.DemandePlats.Select(dp => new DemandePlatDto
                {
                    Id = dp.Id,
                    DemandeMenuId = dp.DemandeMenuId,
                    NomPlatSouhaite = dp.NomPlatSouhaite,
                    DescriptionSouhaitee = dp.DescriptionSouhaitee,
                    TypePlat = dp.TypePlat,
                    UniteSouhaitee = dp.UniteSouhaitee,
                    PrixMaximal = dp.PrixMaximal,
                    QuantiteEstimee = dp.QuantiteEstimee,
                    SpecificationsSpeciales = dp.SpecificationsSpeciales,
                    IsObligatoire = dp.IsObligatoire,
                    Status = dp.Status,
                    ArticleProposedId = dp.ArticleProposedId,
                    CommentairesFournisseur = dp.CommentairesFournisseur,
                    CreatedAt = dp.CreatedAt,
                    UpdatedAt = dp.UpdatedAt
                }).ToList(),
                Reponses = demande.Reponses.Select(r => new DemandeMenuReponseDto
                {
                    Id = r.Id,
                    DemandeMenuId = r.DemandeMenuId,
                    MenuProposedId = r.MenuProposedId,
                    NomMenuPropose = r.NomMenuPropose,
                    DescriptionMenuPropose = r.DescriptionMenuPropose,
                    PrixTotal = r.PrixTotal,
                    CommentairesFournisseur = r.CommentairesFournisseur,
                    IsAcceptedByAdmin = r.IsAcceptedByAdmin,
                    DateProposition = r.DateProposition,
                    DateAcceptation = r.DateAcceptation,
                    CommentairesAcceptation = r.CommentairesAcceptation
                }).ToList()
            };
        }
    }
}
