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
                    Article = new ArticleDto
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
                    }
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
        /// Supprime un BL
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

            _context.BonsLivraison.Remove(bon);
            await _context.SaveChangesAsync();

            return NoContent();
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
                            VolId = bonLivraison.VolId,
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
                        VolId = bonLivraison.VolId,
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
                if (!bcpArticles.ContainsKey(blLigne.ArticleId))
                {
                    var ecart = new Ecart
                    {
                        VolId = bonLivraison.VolId,
                        ArticleId = blLigne.ArticleId,
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

        private bool BonLivraisonExists(int id)
        {
            return _context.BonsLivraison.Any(e => e.Id == id);
        }
    }
}
