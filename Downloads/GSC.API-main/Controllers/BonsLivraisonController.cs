using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BonsLivraisonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BonsLivraisonController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les bons de livraison avec pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BonLivraisonDto>>> GetBonsLivraison([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var bons = await _context.BonsLivraison
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BonLivraisonDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    VolId = b.VolId,
                    BonCommandePrevisionnelId = b.BonCommandePrevisionnelId,
                    DemandeMenuId = b.DemandeMenuId,
                    DateLivraison = b.DateLivraison,
                    Status = b.Status,
                    Fournisseur = b.Fournisseur,
                    Livreur = b.Livreur,
                    Commentaires = b.Commentaires,
                    MontantTotal = b.MontantTotal,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    ValidatedBy = b.ValidatedBy,
                    ValidationDate = b.ValidationDate
                })
                .ToListAsync();

            return Ok(bons);
        }

        /// <summary>
        /// DEBUG: Liste tous les BL avec leur DemandeMenuId
        /// </summary>
        [HttpGet("debug/all")]
        public async Task<ActionResult> DebugGetAllBL()
        {
            var bls = await _context.BonsLivraison
                .Include(b => b.Lignes)
                .Select(b => new {
                    b.Id,
                    b.Numero,
                    b.DemandeMenuId,
                    b.DateLivraison,
                    LignesCount = b.Lignes.Count,
                    Lignes = b.Lignes.Select(l => new {
                        l.Id,
                        l.DemandePlatId,
                        l.NomArticle,
                        l.QuantiteLivree
                    })
                })
                .ToListAsync();
            
            return Ok(bls);
        }

        /// <summary>
        /// Récupère un BL par ID avec tous les détails
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BonLivraisonDetailsDto>> GetBonLivraison(int id)
        {
            var bon = await _context.BonsLivraison
                .Include(b => b.Vol)
                .Include(b => b.BonCommandePrevisionnel)
                .Include(b => b.Lignes)
                    .ThenInclude(l => l.Article)
                .Include(b => b.Ecarts)
                    .ThenInclude(e => e.Article)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bon == null)
            {
                return NotFound($"Bon de livraison avec l'ID {id} non trouvé.");
            }

            var bonDto = new BonLivraisonDetailsDto
            {
                Id = bon.Id,
                Numero = bon.Numero,
                VolId = bon.VolId,
                BonCommandePrevisionnelId = bon.BonCommandePrevisionnelId,
                DateLivraison = bon.DateLivraison,
                Status = bon.Status,
                Fournisseur = bon.Fournisseur,
                Livreur = bon.Livreur,
                Commentaires = bon.Commentaires,
                MontantTotal = bon.MontantTotal,
                CreatedAt = bon.CreatedAt,
                UpdatedAt = bon.UpdatedAt,
                ValidatedBy = bon.ValidatedBy,
                ValidationDate = bon.ValidationDate,
                Vol = new VolDto
                {
                    Id = bon.Vol.Id,
                    FlightNumber = bon.Vol.FlightNumber,
                    FlightDate = bon.Vol.FlightDate,
                    DepartureTime = bon.Vol.DepartureTime,
                    ArrivalTime = bon.Vol.ArrivalTime,
                    Aircraft = bon.Vol.Aircraft,
                    Origin = bon.Vol.Origin,
                    Destination = bon.Vol.Destination,
                    Zone = bon.Vol.Zone,
                    EstimatedPassengers = bon.Vol.EstimatedPassengers,
                    ActualPassengers = bon.Vol.ActualPassengers,
                    Duration = bon.Vol.Duration,
                    Season = bon.Vol.Season,
                    CreatedAt = bon.Vol.CreatedAt,
                    UpdatedAt = bon.Vol.UpdatedAt
                },
                BonCommandePrevisionnel = bon.BonCommandePrevisionnel != null ? new BonCommandePrevisionnelDto
                {
                    Id = bon.BonCommandePrevisionnel.Id,
                    Numero = bon.BonCommandePrevisionnel.Numero,
                    VolId = bon.BonCommandePrevisionnel.VolId,
                    DateCommande = bon.BonCommandePrevisionnel.DateCommande,
                    Status = bon.BonCommandePrevisionnel.Status,
                    Fournisseur = bon.BonCommandePrevisionnel.Fournisseur,
                    MontantTotal = bon.BonCommandePrevisionnel.MontantTotal,
                    Commentaires = bon.BonCommandePrevisionnel.Commentaires,
                    CreatedAt = bon.BonCommandePrevisionnel.CreatedAt,
                    UpdatedAt = bon.BonCommandePrevisionnel.UpdatedAt,
                    CreatedBy = bon.BonCommandePrevisionnel.CreatedBy
                } : null,
                Lignes = bon.Lignes.Select(l => new BonLivraisonLigneDto
                {
                    Id = l.Id,
                    BonLivraisonId = l.BonLivraisonId,
                    ArticleId = l.ArticleId,
                    QuantiteLivree = l.QuantiteLivree,
                    PrixUnitaire = l.PrixUnitaire,
                    MontantLigne = l.MontantLigne,
                    Commentaires = l.Commentaires,
                    Article = l.Article != null ? new ArticleDto
                    {
                        Id = l.Article.Id,
                        Code = l.Article.Code,
                        Name = l.Article.Name,
                        Description = l.Article.Description,
                        Type = l.Article.Type,
                        Unit = l.Article.Unit,
                        UnitPrice = l.Article.UnitPrice,
                        Supplier = l.Article.Supplier,
                        IsActive = l.Article.IsActive,
                        CreatedAt = l.Article.CreatedAt,
                        UpdatedAt = l.Article.UpdatedAt
                    } : null
                }).ToList(),
                Ecarts = bon.Ecarts.Select(e => new EcartDto
                {
                    Id = e.Id,
                    VolId = e.VolId,
                    ArticleId = e.ArticleId,
                    BonCommandePrevisionnelId = e.BonCommandePrevisionnelId,
                    BonLivraisonId = e.BonLivraisonId,
                    TypeEcart = e.TypeEcart,
                    Status = e.Status,
                    QuantiteCommandee = e.QuantiteCommandee,
                    QuantiteLivree = e.QuantiteLivree,
                    EcartQuantite = e.EcartQuantite,
                    PrixCommande = e.PrixCommande,
                    PrixLivraison = e.PrixLivraison,
                    EcartMontant = e.EcartMontant,
                    Description = e.Description,
                    ActionCorrective = e.ActionCorrective,
                    ResponsableTraitement = e.ResponsableTraitement,
                    DateDetection = e.DateDetection,
                    DateResolution = e.DateResolution,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList()
            };

            return Ok(bonDto);
        }

        /// <summary>
        /// Crée un nouveau BL avec ses lignes
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BonLivraisonDto>> CreateBonLivraison(CreateBonLivraisonDto createBonDto)
        {
            // Vérifier que le vol existe
            var vol = await _context.Vols.FindAsync(createBonDto.VolId);
            if (vol == null)
            {
                return NotFound($"Vol avec l'ID {createBonDto.VolId} non trouvé.");
            }

            // Vérifier l'unicité du numéro
            if (await _context.BonsLivraison.AnyAsync(b => b.Numero == createBonDto.Numero))
            {
                return BadRequest($"Un BL avec le numéro '{createBonDto.Numero}' existe déjà.");
            }

            // Vérifier le BCP s'il est spécifié
            if (createBonDto.BonCommandePrevisionnelId.HasValue)
            {
                var bcp = await _context.BonsCommandePrevisionnels.FindAsync(createBonDto.BonCommandePrevisionnelId.Value);
                if (bcp == null)
                {
                    return NotFound($"BCP avec l'ID {createBonDto.BonCommandePrevisionnelId.Value} non trouvé.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var bon = new BonLivraison
                {
                    Numero = createBonDto.Numero,
                    VolId = createBonDto.VolId,
                    BonCommandePrevisionnelId = createBonDto.BonCommandePrevisionnelId,
                    DateLivraison = createBonDto.DateLivraison,
                    Status = StatusBL.Recu,
                    Fournisseur = createBonDto.Fournisseur,
                    Livreur = createBonDto.Livreur,
                    Commentaires = createBonDto.Commentaires,
                    MontantTotal = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BonsLivraison.Add(bon);
                await _context.SaveChangesAsync();

                decimal montantTotal = 0;

                // Ajouter les lignes du BL
                foreach (var ligneDto in createBonDto.Lignes)
                {
                    var article = await _context.Articles.FindAsync(ligneDto.ArticleId);
                    if (article == null)
                    {
                        throw new ArgumentException($"Article avec l'ID {ligneDto.ArticleId} non trouvé.");
                    }

                    var prixUnitaire = ligneDto.PrixUnitaire > 0 ? ligneDto.PrixUnitaire : article.UnitPrice;
                    var montantLigne = ligneDto.QuantiteLivree * prixUnitaire;

                    var ligne = new BonLivraisonLigne
                    {
                        BonLivraisonId = bon.Id,
                        ArticleId = ligneDto.ArticleId,
                        QuantiteLivree = ligneDto.QuantiteLivree,
                        PrixUnitaire = prixUnitaire,
                        MontantLigne = montantLigne,
                        Commentaires = ligneDto.Commentaires
                    };

                    _context.BonLivraisonLignes.Add(ligne);
                    montantTotal += montantLigne;
                }

                bon.MontantTotal = montantTotal;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var bonDto = new BonLivraisonDto
                {
                    Id = bon.Id,
                    Numero = bon.Numero,
                    VolId = bon.VolId,
                    BonCommandePrevisionnelId = bon.BonCommandePrevisionnelId,
                    DateLivraison = bon.DateLivraison,
                    Status = bon.Status,
                    Fournisseur = bon.Fournisseur,
                    Livreur = bon.Livreur,
                    Commentaires = bon.Commentaires,
                    MontantTotal = bon.MontantTotal,
                    CreatedAt = bon.CreatedAt,
                    UpdatedAt = bon.UpdatedAt,
                    ValidatedBy = bon.ValidatedBy,
                    ValidationDate = bon.ValidationDate
                };

                return CreatedAtAction(nameof(GetBonLivraison), new { id = bon.Id }, bonDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Met à jour un BL existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBonLivraison(int id, UpdateBonLivraisonDto updateBonDto)
        {
            var bon = await _context.BonsLivraison.FindAsync(id);
            if (bon == null)
            {
                return NotFound($"BL avec l'ID {id} non trouvé.");
            }

            if (!string.IsNullOrEmpty(updateBonDto.Numero) && updateBonDto.Numero != bon.Numero)
            {
                if (await _context.BonsLivraison.AnyAsync(b => b.Numero == updateBonDto.Numero && b.Id != id))
                {
                    return BadRequest($"Un BL avec le numéro '{updateBonDto.Numero}' existe déjà.");
                }
                bon.Numero = updateBonDto.Numero;
            }

            if (updateBonDto.DateLivraison.HasValue)
                bon.DateLivraison = updateBonDto.DateLivraison.Value;
            if (updateBonDto.Status.HasValue)
                bon.Status = updateBonDto.Status.Value;
            if (updateBonDto.Fournisseur != null)
                bon.Fournisseur = updateBonDto.Fournisseur;
            if (updateBonDto.Livreur != null)
                bon.Livreur = updateBonDto.Livreur;
            if (updateBonDto.Commentaires != null)
                bon.Commentaires = updateBonDto.Commentaires;

            bon.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BonLivraisonExists(id))
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
        /// Supprime un BL et remet la demande liée en attente
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBonLivraison(int id)
        {
            var bon = await _context.BonsLivraison
                .Include(b => b.Lignes)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bon == null)
            {
                return NotFound($"BL avec l'ID {id} non trouvé.");
            }

            if (bon.Status == StatusBL.Valide)
            {
                return BadRequest("Un BL validé ne peut pas être supprimé.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Si lié à une demande, remettre la demande en attente
                if (bon.DemandeMenuId.HasValue)
                {
                    var demande = await _context.DemandesMenu.FindAsync(bon.DemandeMenuId.Value);
                    if (demande != null)
                    {
                        demande.Status = StatusDemande.EnAttente;
                        demande.DateReponse = null;
                        demande.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Supprimer les lignes puis le BL
                _context.BonLivraisonLignes.RemoveRange(bon.Lignes);
                _context.BonsLivraison.Remove(bon);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Bon de Livraison {bon.Numero} supprimé avec succès.",
                    deletedId = id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Erreur lors de la suppression: {ex.Message}");
            }
        }

        /// <summary>
        /// Valide un bon de livraison et génère automatiquement les écarts
        /// </summary>
        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateBonLivraison(int id)
        {
            var bon = await _context.BonsLivraison
                .Include(b => b.Vol)
                .Include(b => b.BonCommandePrevisionnel)
                    .ThenInclude(bcp => bcp!.Lignes)
                        .ThenInclude(l => l.Article)
                .Include(b => b.Lignes)
                    .ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bon == null)
            {
                return NotFound($"BL avec l'ID {id} non trouvé.");
            }

            if (bon.Status == StatusBL.Valide)
            {
                return BadRequest("Ce BL est déjà validé.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                bon.Status = StatusBL.Valide;
                bon.ValidatedBy = HttpContext.User?.Identity?.Name;
                bon.ValidationDate = DateTime.UtcNow;
                bon.UpdatedAt = DateTime.UtcNow;

                // Générer les écarts si un BCP est associé
                if (bon.BonCommandePrevisionnel != null)
                {
                    await GenerateEcarts(bon);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "BL validé avec succès", ecartsGeneres = bon.BonCommandePrevisionnel != null });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Génère automatiquement les écarts entre BCP et BL
        /// </summary>
        private async Task GenerateEcarts(BonLivraison bonLivraison)
        {
            if (bonLivraison.BonCommandePrevisionnel == null) return;

            var bcpArticles = bonLivraison.BonCommandePrevisionnel.Lignes.ToDictionary(l => l.ArticleId, l => l);
            var blArticles = bonLivraison.Lignes.ToDictionary(l => l.ArticleId, l => l);

            // Écarts pour les articles commandés
            foreach (var bcpLigne in bonLivraison.BonCommandePrevisionnel.Lignes)
            {
                if (blArticles.TryGetValue(bcpLigne.ArticleId, out var blLigne))
                {
                    // Article présent dans les deux
                    var ecartQuantite = blLigne.QuantiteLivree - bcpLigne.QuantiteCommandee;
                    var ecartMontant = blLigne.MontantLigne - bcpLigne.MontantLigne;

                    if (ecartQuantite != 0 || Math.Abs(ecartMontant) > 0.01m)
                    {
                        var typeEcart = ecartQuantite > 0 ? TypeEcart.QuantiteSuperieure : 
                                       ecartQuantite < 0 ? TypeEcart.QuantiteInferieure : 
                                       TypeEcart.PrixDifferent;

                        var ecart = new Ecart
                        {
                            VolId = bonLivraison.VolId ?? bonLivraison.BonCommandePrevisionnel.VolId,
                            ArticleId = bcpLigne.ArticleId,
                            BonCommandePrevisionnelId = bonLivraison.BonCommandePrevisionnelId,
                            BonLivraisonId = bonLivraison.Id,
                            TypeEcart = typeEcart,
                            Status = StatusEcart.EnAttente,
                            QuantiteCommandee = bcpLigne.QuantiteCommandee,
                            QuantiteLivree = blLigne.QuantiteLivree,
                            EcartQuantite = ecartQuantite,
                            PrixCommande = bcpLigne.PrixUnitaire,
                            PrixLivraison = blLigne.PrixUnitaire,
                            EcartMontant = ecartMontant,
                            Description = $"Écart détecté lors de la validation du BL {bonLivraison.Numero}",
                            DateDetection = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Ecarts.Add(ecart);
                    }
                }
                else
                {
                    // Article commandé mais pas livré
                    var ecart = new Ecart
                    {
                        VolId = bonLivraison.VolId ?? bonLivraison.BonCommandePrevisionnel.VolId,
                        ArticleId = bcpLigne.ArticleId,
                        BonCommandePrevisionnelId = bonLivraison.BonCommandePrevisionnelId,
                        BonLivraisonId = bonLivraison.Id,
                        TypeEcart = TypeEcart.ArticleManquant,
                        Status = StatusEcart.EnAttente,
                        QuantiteCommandee = bcpLigne.QuantiteCommandee,
                        QuantiteLivree = 0,
                        EcartQuantite = -bcpLigne.QuantiteCommandee,
                        PrixCommande = bcpLigne.PrixUnitaire,
                        PrixLivraison = 0,
                        EcartMontant = -bcpLigne.MontantLigne,
                        Description = $"Article commandé mais non livré - BL {bonLivraison.Numero}",
                        DateDetection = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Ecarts.Add(ecart);
                }
            }

            // Écarts pour les articles livrés mais non commandés
            foreach (var blLigne in bonLivraison.Lignes)
            {
                // Skip if no ArticleId (can't create Ecart without an article)
                if (!blLigne.ArticleId.HasValue) continue;

                if (!bcpArticles.ContainsKey(blLigne.ArticleId.Value))
                {
                    var ecart = new Ecart
                    {
                        VolId = bonLivraison.VolId ?? bonLivraison.BonCommandePrevisionnel.VolId,
                        ArticleId = blLigne.ArticleId.Value,
                        BonCommandePrevisionnelId = bonLivraison.BonCommandePrevisionnelId,
                        BonLivraisonId = bonLivraison.Id,
                        TypeEcart = TypeEcart.ArticleEnPlus,
                        Status = StatusEcart.EnAttente,
                        QuantiteCommandee = 0,
                        QuantiteLivree = blLigne.QuantiteLivree,
                        EcartQuantite = blLigne.QuantiteLivree,
                        PrixCommande = 0,
                        PrixLivraison = blLigne.PrixUnitaire,
                        EcartMontant = blLigne.MontantLigne,
                        Description = $"Article livré mais non commandé - BL {bonLivraison.Numero}",
                        DateDetection = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Ecarts.Add(ecart);
                }
            }
        }

        /// <summary>
        /// Change le statut d'un BL
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBlStatus(int id, [FromBody] StatusBL newStatus)
        {
            var bon = await _context.BonsLivraison.FindAsync(id);
            if (bon == null)
            {
                return NotFound($"BL avec l'ID {id} non trouvé.");
            }

            bon.Status = newStatus;
            bon.UpdatedAt = DateTime.UtcNow;

            if (newStatus == StatusBL.Valide)
            {
                bon.ValidatedBy = HttpContext.User?.Identity?.Name;
                bon.ValidationDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Recherche des BL par critères
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BonLivraisonDto>>> SearchBl(
            [FromQuery] string? numero,
            [FromQuery] int? volId,
            [FromQuery] int? bcpId,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] StatusBL? status,
            [FromQuery] string? fournisseur,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.BonsLivraison.AsQueryable();

            if (!string.IsNullOrEmpty(numero))
                query = query.Where(b => b.Numero.Contains(numero));

            if (volId.HasValue)
                query = query.Where(b => b.VolId == volId.Value);

            if (bcpId.HasValue)
                query = query.Where(b => b.BonCommandePrevisionnelId == bcpId.Value);

            if (dateDebut.HasValue)
                query = query.Where(b => b.DateLivraison >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(b => b.DateLivraison <= dateFin.Value);

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            if (!string.IsNullOrEmpty(fournisseur))
                query = query.Where(b => b.Fournisseur.Contains(fournisseur));

            var bons = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BonLivraisonDto
                {
                    Id = b.Id,
                    Numero = b.Numero,
                    VolId = b.VolId,
                    BonCommandePrevisionnelId = b.BonCommandePrevisionnelId,
                    DateLivraison = b.DateLivraison,
                    Status = b.Status,
                    Fournisseur = b.Fournisseur,
                    Livreur = b.Livreur,
                    Commentaires = b.Commentaires,
                    MontantTotal = b.MontantTotal,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    ValidatedBy = b.ValidatedBy,
                    ValidationDate = b.ValidationDate
                })
                .ToListAsync();

            return Ok(bons);
        }

        /// <summary>
        /// Récupère les détails d'une demande pour créer un bon de livraison
        /// </summary>
        [HttpGet("demande/{demandeId}/for-livraison")]
        public async Task<ActionResult> GetDemandeForLivraison(int demandeId)
        {
            var demande = await _context.DemandesMenu
                .Include(d => d.DemandePlats)
                    .ThenInclude(dp => dp.ArticleProposed)
                .Include(d => d.DemandeParUser)
                .Include(d => d.AssigneAFournisseur)
                .FirstOrDefaultAsync(d => d.Id == demandeId);

            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {demandeId} non trouvée.");
            }

            // Vérifier si l'utilisateur est le fournisseur assigné (sauf pour les admins)
            if (User.IsInRole("Fournisseur"))
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (demande.AssigneAFournisseurId != userId)
                {
                    return Forbid();
                }
            }

            var result = new
            {
                demandeId = demande.Id,
                numero = demande.Numero,
                titre = demande.Titre,
                description = demande.Description,
                dateDemande = demande.DateDemande,
                dateLimite = demande.DateLimite,
                status = demande.Status.ToString(),
                demandeParUser = demande.DemandeParUser != null ? new
                {
                    id = demande.DemandeParUser.Id,
                    nom = $"{demande.DemandeParUser.FirstName} {demande.DemandeParUser.LastName}",
                    email = demande.DemandeParUser.Email
                } : null,
                plats = demande.DemandePlats.Select(dp => new
                {
                    id = dp.Id,
                    nomPlatSouhaite = dp.NomPlatSouhaite,
                    description = dp.DescriptionSouhaitee,
                    typePlat = dp.TypePlat.ToString(),
                    quantiteEstimee = dp.QuantiteEstimee,
                    uniteSouhaitee = dp.UniteSouhaitee,
                    prixMaximal = dp.PrixMaximal,
                    specificationsSpeciales = dp.SpecificationsSpeciales,
                    isObligatoire = dp.IsObligatoire,
                    articleProposed = dp.ArticleProposed != null ? new
                    {
                        id = dp.ArticleProposed.Id,
                        code = dp.ArticleProposed.Code,
                        name = dp.ArticleProposed.Name,
                        unitPrice = dp.ArticleProposed.UnitPrice,
                        unit = dp.ArticleProposed.Unit
                    } : null
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Accepte une demande de menu et crée le bon de livraison correspondant
        /// </summary>
        [HttpPost("accept-demande")]
        public async Task<ActionResult<BonLivraisonDetailsDto>> AcceptDemande([FromBody] AcceptDemandeDto acceptDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                return BadRequest($"Validation failed: {errors}");
            }

            if (acceptDto.DemandeId == null)
            {
                return BadRequest("Validation failed: Le champ 'demandeId' est requis.");
            }

            var demande = await _context.DemandesMenu
                .Include(d => d.DemandePlats)
                .Include(d => d.AssigneAFournisseur)
                .FirstOrDefaultAsync(d => d.Id == acceptDto.DemandeId.Value);

            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {acceptDto.DemandeId} non trouvée.");
            }

            if (demande.Status == StatusDemande.Acceptee)
            {
                return BadRequest("Cette demande a déjà été acceptée.");
            }

            // Vérifier l'unicité du numéro de BL
            if (await _context.BonsLivraison.AnyAsync(b => b.Numero == acceptDto.NumeroBL))
            {
                return BadRequest($"Un BL avec le numéro '{acceptDto.NumeroBL}' existe déjà.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            int createdBonId;

            try
            {
                // 1. Mettre à jour le statut de la demande
                demande.Status = StatusDemande.Acceptee;
                demande.DateReponse = DateTime.UtcNow;
                demande.UpdatedAt = DateTime.UtcNow;

                // 2. Créer le Bon de Livraison
                var bon = new BonLivraison
                {
                    Numero = acceptDto.NumeroBL,
                    DemandeMenuId = demande.Id,
                    DateLivraison = acceptDto.DateLivraison,
                    Status = StatusBL.Recu,
                    Fournisseur = demande.AssigneAFournisseur?.LastName ?? "Fournisseur Inconnu",
                    Livreur = acceptDto.Livreur,
                    Commentaires = acceptDto.Commentaires,
                    MontantTotal = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BonsLivraison.Add(bon);
                await _context.SaveChangesAsync();

                decimal montantTotal = 0;

                // 3. Créer les lignes du BL
                foreach (var ligneDto in acceptDto.Lignes)
                {
                    var demandePlat = demande.DemandePlats.FirstOrDefault(p => p.Id == ligneDto.DemandePlatId);
                    var montantLigne = ligneDto.QuantiteLivree * ligneDto.PrixUnitaire;

                    var ligne = new BonLivraisonLigne
                    {
                        BonLivraisonId = bon.Id,
                        ArticleId = (ligneDto.ArticleId == 0) ? null : ligneDto.ArticleId,
                        DemandePlatId = ligneDto.DemandePlatId,
                        NomArticle = !string.IsNullOrEmpty(ligneDto.NomArticle) ? ligneDto.NomArticle : demandePlat?.NomPlatSouhaite,
                        QuantiteLivree = ligneDto.QuantiteLivree,
                        PrixUnitaire = ligneDto.PrixUnitaire,
                        MontantLigne = montantLigne,
                        Commentaires = demandePlat != null ? $"Pour: {demandePlat.NomPlatSouhaite}" : null
                    };

                    _context.BonLivraisonLignes.Add(ligne);
                    montantTotal += montantLigne;
                }

                bon.MontantTotal = montantTotal;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                createdBonId = bon.Id;
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { /* ignore rollback errors */ }
                return StatusCode(500, $"Erreur lors de l'acceptation de la demande: {ex.Message}");
            }

            // Retourner le DTO du BL créé (outside transaction)
            return await GetBonLivraison(createdBonId);
        }

        /// <summary>
        /// Compare la demande initiale avec le bon de livraison généré
        /// </summary>
        [HttpGet("compare/{demandeId}")]
        public async Task<ActionResult<DemandeLivraisonComparisonDto>> GetComparison(int demandeId)
        {
            var demande = await _context.DemandesMenu
                .Include(d => d.DemandePlats)
                    .ThenInclude(dp => dp.ArticleProposed)
                .FirstOrDefaultAsync(d => d.Id == demandeId);

            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {demandeId} non trouvée.");
            }

            var bon = await _context.BonsLivraison
                .Include(b => b.Lignes)
                    .ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(b => b.DemandeMenuId == demandeId);

            // DEBUG: Log what we found
            Console.WriteLine($"[DEBUG] Comparison for DemandeId={demandeId}");
            Console.WriteLine($"[DEBUG] BonLivraison found: {(bon != null ? $"Id={bon.Id}, Numero={bon.Numero}, LignesCount={bon.Lignes.Count}" : "NULL")}");
            if (bon != null)
            {
                foreach (var l in bon.Lignes)
                {
                    Console.WriteLine($"[DEBUG] BL Ligne: Id={l.Id}, DemandePlatId={l.DemandePlatId}, ArticleId={l.ArticleId}, NomArticle={l.NomArticle}, Qty={l.QuantiteLivree}");
                }
            }
            Console.WriteLine($"[DEBUG] DemandePlats count: {demande.DemandePlats.Count}");
            foreach (var p in demande.DemandePlats)
            {
                Console.WriteLine($"[DEBUG] DemandePlat: Id={p.Id}, Nom={p.NomPlatSouhaite}, Qty={p.QuantiteEstimee}");
            }

            var comparison = new DemandeLivraisonComparisonDto
            {
                DemandeId = demande.Id,
                DemandeNumero = demande.Numero,
                DemandeTitre = demande.Titre,
                DateDemande = demande.DateDemande,
                BonLivraisonId = bon?.Id,
                BonLivraisonNumero = bon?.Numero,
                DateLivraison = bon?.DateLivraison,
                StatusLivraison = bon?.Status
            };

            // Logique de comparaison
            // On itère sur les plats demandés
            foreach (var plat in demande.DemandePlats)
            {
                var ligneComparison = new LigneComparisonDto
                {
                    NomArticle = plat.NomPlatSouhaite,
                    QuantiteDemandee = plat.QuantiteEstimee ?? 0,
                    UniteDemandee = plat.UniteSouhaitee
                };

                // Essayer de trouver la ligne correspondante dans le BL
                BonLivraisonLigne? ligneBl = null;
                if (bon != null)
                {
                     // Match by DemandePlatId (new way)
                     ligneBl = bon.Lignes.FirstOrDefault(l => l.DemandePlatId == plat.Id);
                     
                     // Fallback to ArticleId match (old way) if not found
                     if (ligneBl == null && plat.ArticleProposedId.HasValue)
                     {
                         ligneBl = bon.Lignes.FirstOrDefault(l => l.ArticleId == plat.ArticleProposedId.Value);
                     }
                }

                if (ligneBl != null)
                {
                    ligneComparison.ArticleId = ligneBl.ArticleId;
                    ligneComparison.CodeArticle = ligneBl.Article?.Code;
                    ligneComparison.NomArticle = !string.IsNullOrEmpty(ligneBl.NomArticle) ? ligneBl.NomArticle : (ligneBl.Article?.Name ?? ligneComparison.NomArticle);
                    ligneComparison.QuantiteLivree = ligneBl.QuantiteLivree;
                    ligneComparison.PrixUnitaire = ligneBl.PrixUnitaire;
                    ligneComparison.MontantTotal = ligneBl.MontantLigne;
                }

                // Calcul de l'écart
                ligneComparison.EcartQuantite = ligneComparison.QuantiteLivree - ligneComparison.QuantiteDemandee;

                if (ligneComparison.QuantiteLivree == 0 && ligneComparison.QuantiteDemandee > 0)
                {
                    ligneComparison.Status = "Manquant";
                }
                else if (ligneComparison.QuantiteLivree < ligneComparison.QuantiteDemandee)
                {
                    ligneComparison.Status = "Partiel";
                }
                else if (ligneComparison.QuantiteLivree > ligneComparison.QuantiteDemandee)
                {
                    ligneComparison.Status = "Surplus";
                }
                else
                {
                    ligneComparison.Status = "Conforme";
                }

                comparison.Lignes.Add(ligneComparison);
            }

            // Ajouter les lignes du BL qui ne correspondent à aucun plat demandé (Surplus non prévu)
            if (bon != null)
            {
                var articleIdsDemandes = demande.DemandePlats
                    .Where(p => p.ArticleProposedId.HasValue)
                    .Select(p => p.ArticleProposedId.Value)
                    .ToList();

                var lignesNonDemandees = bon.Lignes.Where(l => 
                    !l.DemandePlatId.HasValue &&
                    (!l.ArticleId.HasValue || !articleIdsDemandes.Contains(l.ArticleId.Value)));

                foreach (var ligne in lignesNonDemandees)
                {
                    comparison.Lignes.Add(new LigneComparisonDto
                    {
                        ArticleId = ligne.ArticleId,
                        NomArticle = !string.IsNullOrEmpty(ligne.NomArticle) ? ligne.NomArticle : (ligne.Article?.Name ?? "Article Inconnu"),
                        CodeArticle = ligne.Article?.Code,
                        QuantiteDemandee = 0,
                        QuantiteLivree = ligne.QuantiteLivree,
                        PrixUnitaire = ligne.PrixUnitaire,
                        MontantTotal = ligne.MontantLigne,
                        EcartQuantite = ligne.QuantiteLivree,
                        Status = "Non Commandé"
                    });
                }
            }

            return Ok(comparison);
        }


        private bool BonLivraisonExists(int id)
        {
            return _context.BonsLivraison.Any(e => e.Id == id);
        }
    }
}
