using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;
using System.Security.Claims;
using GsC.API.Services;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FournisseursController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public FournisseursController(ApplicationDbContext context, UserManager<User> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        /// <summary>
        /// Récupère tous les fournisseurs (Admin uniquement)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<IEnumerable<FournisseurDto>>> GetFournisseurs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var fournisseurs = await _context.Fournisseurs
                .Include(f => f.User)
                .Where(f => f.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FournisseurDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    UserName = f.User.UserName!,
                    UserEmail = f.User.Email!,
                    CompanyName = f.CompanyName,
                    Address = f.Address,
                    Phone = f.Phone,
                    ContactEmail = f.ContactEmail,
                    ContactPerson = f.ContactPerson,
                    Siret = f.Siret,
                    NumeroTVA = f.NumeroTVA,
                    Specialites = f.Specialites,
                    IsActive = f.IsActive,
                    IsVerified = f.IsVerified,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();

            return Ok(fournisseurs);
        }

        /// <summary>
        /// Récupère un fournisseur par ID (Admin ou le fournisseur lui-même)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FournisseurDto>> GetFournisseur(int id)
        {
            var fournisseur = await _context.Fournisseurs
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fournisseur == null)
            {
                return NotFound($"Fournisseur avec l'ID {id} non trouvé.");
            }

            // Vérifier les permissions
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("Manager");
            
            if (!isAdmin && fournisseur.UserId != currentUserId)
            {
                return Forbid("Vous n'avez pas l'autorisation d'accéder à ces informations.");
            }

            var fournisseurDto = new FournisseurDto
            {
                Id = fournisseur.Id,
                UserId = fournisseur.UserId,
                UserName = fournisseur.User.UserName!,
                UserEmail = fournisseur.User.Email!,
                CompanyName = fournisseur.CompanyName,
                Address = fournisseur.Address,
                Phone = fournisseur.Phone,
                ContactEmail = fournisseur.ContactEmail,
                ContactPerson = fournisseur.ContactPerson,
                Siret = fournisseur.Siret,
                NumeroTVA = fournisseur.NumeroTVA,
                Specialites = fournisseur.Specialites,
                IsActive = fournisseur.IsActive,
                IsVerified = fournisseur.IsVerified,
                CreatedAt = fournisseur.CreatedAt,
                UpdatedAt = fournisseur.UpdatedAt
            };

            return Ok(fournisseurDto);
        }

        /// <summary>
        /// Crée un nouveau compte fournisseur avec utilisateur (Admin uniquement)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<FournisseurDto>> CreateFournisseur(CreateFournisseurDto createFournisseurDto)
        {
            // Vérifier si l'email existe déjà
            var existingUser = await _userManager.FindByEmailAsync(createFournisseurDto.UserEmail);
            if (existingUser != null)
            {
                return BadRequest($"Un utilisateur avec l'email {createFournisseurDto.UserEmail} existe déjà.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Générer un mot de passe aléatoire
                string password = GenerateRandomPassword();

                // Créer l'utilisateur
                var user = new User
                {
                    UserName = createFournisseurDto.UserEmail,
                    Email = createFournisseurDto.UserEmail,
                    FirstName = createFournisseurDto.UserFirstName,
                    LastName = createFournisseurDto.UserLastName,
                    EmailConfirmed = true
                };

                // Utiliser le mot de passe généré
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // LOG pour développement - RETIRER EN PRODUCTION
                Console.WriteLine($"🔑 COMPTE FOURNISSEUR CRÉÉ:");
                Console.WriteLine($"📧 Email: {user.Email}");
                Console.WriteLine($"🔐 Mot de passe: {password}");
                Console.WriteLine($"🏢 Entreprise: {createFournisseurDto.CompanyName}");

                // Assigner le rôle Fournisseur
                await _userManager.AddToRoleAsync(user, "Fournisseur");

                // Créer le profil fournisseur
                var fournisseur = new Fournisseur
                {
                    UserId = user.Id,
                    CompanyName = createFournisseurDto.CompanyName,
                    Address = createFournisseurDto.Address,
                    Phone = createFournisseurDto.Phone,
                    ContactEmail = createFournisseurDto.ContactEmail,
                    ContactPerson = createFournisseurDto.ContactPerson,
                    Siret = createFournisseurDto.Siret,
                    NumeroTVA = createFournisseurDto.NumeroTVA,
                    Specialites = createFournisseurDto.Specialites,
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Fournisseurs.Add(fournisseur);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Envoyer email avec les informations de connexion
                await _emailService.SendSupplierCredentialsAsync(
                    user.Email!, 
                    user.FirstName ?? "Partenaire", 
                    password, 
                    fournisseur.CompanyName
                );

                var fournisseurDto = new FournisseurDto
                {
                    Id = fournisseur.Id,
                    UserId = fournisseur.UserId,
                    UserName = user.UserName!,
                    UserEmail = user.Email!,
                    CompanyName = fournisseur.CompanyName,
                    Address = fournisseur.Address,
                    Phone = fournisseur.Phone,
                    ContactEmail = fournisseur.ContactEmail,
                    ContactPerson = fournisseur.ContactPerson,
                    Siret = fournisseur.Siret,
                    NumeroTVA = fournisseur.NumeroTVA,
                    Specialites = fournisseur.Specialites,
                    IsActive = fournisseur.IsActive,
                    IsVerified = fournisseur.IsVerified,
                    CreatedAt = fournisseur.CreatedAt,
                    UpdatedAt = fournisseur.UpdatedAt
                };

                return CreatedAtAction(nameof(GetFournisseur), new { id = fournisseur.Id }, fournisseurDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private string GenerateRandomPassword()
        {
            // Configuration par défaut si _userManager.Options n'est pas accessible ou configuré
            int length = 12;
            bool requireDigit = true;
            bool requireUppercase = true;
            bool requireLowercase = true;
            bool requireNonAlphanumeric = true;

            try 
            {
                // Essayer de récupérer les options si disponibles
                // Note: _userManager.Options peut ne pas être directement accessible selon l'implémentation
                // On utilise des valeurs sûres par défaut
            }
            catch {}
            
            string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lowers = "abcdefghijklmnopqrstuvwxyz";
            string digits = "0123456789";
            string nonAlphanumerics = "!@#$%^&*";

            Random random = new Random();
            
            var passwordChars = new List<char>();
            if (requireUppercase) passwordChars.Add(uppers[random.Next(uppers.Length)]);
            if (requireLowercase) passwordChars.Add(lowers[random.Next(lowers.Length)]);
            if (requireDigit) passwordChars.Add(digits[random.Next(digits.Length)]);
            if (requireNonAlphanumeric) passwordChars.Add(nonAlphanumerics[random.Next(nonAlphanumerics.Length)]);

            string allChars = uppers + lowers + digits + nonAlphanumerics;
            
            while (passwordChars.Count < length)
            {
                passwordChars.Add(allChars[random.Next(allChars.Length)]);
            }

            return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
        }

        /// <summary>
        /// Met à jour un fournisseur (Admin ou le fournisseur lui-même)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFournisseur(int id, UpdateFournisseurDto updateFournisseurDto)
        {
            var fournisseur = await _context.Fournisseurs.FindAsync(id);
            if (fournisseur == null)
            {
                return NotFound($"Fournisseur avec l'ID {id} non trouvé.");
            }

            // Vérifier les permissions
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("Manager");
            
            if (!isAdmin && fournisseur.UserId != currentUserId)
            {
                return Forbid("Vous n'avez pas l'autorisation de modifier ces informations.");
            }

            // Mise à jour des champs
            if (!string.IsNullOrEmpty(updateFournisseurDto.CompanyName))
                fournisseur.CompanyName = updateFournisseurDto.CompanyName;
            if (updateFournisseurDto.Address != null)
                fournisseur.Address = updateFournisseurDto.Address;
            if (updateFournisseurDto.Phone != null)
                fournisseur.Phone = updateFournisseurDto.Phone;
            if (updateFournisseurDto.ContactEmail != null)
                fournisseur.ContactEmail = updateFournisseurDto.ContactEmail;
            if (updateFournisseurDto.ContactPerson != null)
                fournisseur.ContactPerson = updateFournisseurDto.ContactPerson;
            if (updateFournisseurDto.Siret != null)
                fournisseur.Siret = updateFournisseurDto.Siret;
            if (updateFournisseurDto.NumeroTVA != null)
                fournisseur.NumeroTVA = updateFournisseurDto.NumeroTVA;
            if (updateFournisseurDto.Specialites != null)
                fournisseur.Specialites = updateFournisseurDto.Specialites;

            // Seuls les admins peuvent modifier ces champs
            if (isAdmin)
            {
                if (updateFournisseurDto.IsActive.HasValue)
                    fournisseur.IsActive = updateFournisseurDto.IsActive.Value;
                if (updateFournisseurDto.IsVerified.HasValue)
                    fournisseur.IsVerified = updateFournisseurDto.IsVerified.Value;
            }

            fournisseur.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FournisseurExists(id))
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
        /// Vérifie un fournisseur (Admin uniquement)
        /// </summary>
        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> VerifyFournisseur(int id)
        {
            var fournisseur = await _context.Fournisseurs.FindAsync(id);
            if (fournisseur == null)
            {
                return NotFound($"Fournisseur avec l'ID {id} non trouvé.");
            }

            fournisseur.IsVerified = true;
            fournisseur.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Fournisseur vérifié avec succès" });
        }

        /// <summary>
        /// Désactive un fournisseur (Admin uniquement)
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeactivateFournisseur(int id)
        {
            var fournisseur = await _context.Fournisseurs.FindAsync(id);
            if (fournisseur == null)
            {
                return NotFound($"Fournisseur avec l'ID {id} non trouvé.");
            }

            fournisseur.IsActive = false;
            fournisseur.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Fournisseur désactivé avec succès" });
        }

        /// <summary>
        /// Récupère les demandes assignées au fournisseur connecté
        /// </summary>
        [HttpGet("mes-demandes")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<ActionResult<IEnumerable<DemandeMenuDto>>> GetMesDemandes(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] StatusDemande? status = null)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var query = _context.DemandesMenu
                .Include(d => d.DemandeParUser)
                .Include(d => d.DemandePlats)
                    .ThenInclude(dp => dp.ArticleProposed)
                .Include(d => d.Reponses)
                    .ThenInclude(r => r.MenuProposed)
                .Where(d => d.AssigneAFournisseurId == currentUserId && d.IsActive)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

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
                    }).ToList()
                })
                .ToListAsync();

            return Ok(demandes);
        }

        /// <summary>
        /// Refuse une demande de menu (Fournisseur uniquement)
        /// </summary>
        [HttpPost("demandes/{demandeId}/refuser")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<IActionResult> RefuserDemande(int demandeId, [FromBody] string? raison = null)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var demande = await _context.DemandesMenu.FindAsync(demandeId);
            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {demandeId} non trouvée.");
            }

            if (demande.AssigneAFournisseurId != currentUserId)
            {
                return BadRequest("Cette demande ne vous est pas assignée.");
            }

            if (demande.Status == StatusDemande.Completee || demande.Status == StatusDemande.Annulee)
            {
                return BadRequest("Impossible de refuser une demande complétée ou annulée.");
            }

            // Marquer comme refusée mais garder l'assignation pour historique
            demande.Status = StatusDemande.Refusee;
            demande.CommentairesFournisseur = $"REFUSÉ: {raison}";
            demande.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Demande refusée." });
        }

        /// <summary>
        /// Accepte une demande de menu (Fournisseur uniquement)
        /// Crée automatiquement un menu.
        /// </summary>
        [HttpPost("demandes/{demandeId}/accepter")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<IActionResult> AccepterDemande(int demandeId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var demande = await _context.DemandesMenu.FindAsync(demandeId);
            if (demande == null)
            {
                return NotFound($"Demande avec l'ID {demandeId} non trouvée.");
            }

            if (demande.AssigneAFournisseurId != currentUserId)
            {
                return BadRequest("Cette demande ne vous est pas assignée.");
            }

            // Récupérer le fournisseur
            var fournisseur = await _context.Fournisseurs.FirstOrDefaultAsync(f => f.UserId == currentUserId);
            if (fournisseur == null)
            {
                return BadRequest("Profil fournisseur introuvable.");
            }

            // Créer le menu
            var menu = new Menu
            {
                Name = demande.Titre,
                Description = demande.Description ?? "Généré automatiquement",
                TypePassager = "Economy",
                FournisseurId = fournisseur.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Season = "Toutes",
                Zone = "Toutes"
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[DEBUG] Created Menu: Id={menu.Id}, Name='{menu.Name}', Type='{menu.TypePassager}', Season='{menu.Season}'");

            // Mettre à jour la demande
            demande.Status = StatusDemande.Completee;
            demande.CommentairesFournisseur = "Demande acceptée et menu généré.";
            demande.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Demande acceptée et menu généré.", menuId = menu.Id });
        }

        /// <summary>
        /// Répond à une demande de menu (Fournisseur uniquement)
        /// </summary>
        [HttpPost("demandes/{demandeId}/repondre")]
        [Authorize(Roles = "Fournisseur")]
        public async Task<ActionResult<DemandeMenuReponseDto>> RepondreADemande(int demandeId, CreateDemandeMenuReponseDto reponseDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var demande = await _context.DemandesMenu
                .FirstOrDefaultAsync(d => d.Id == demandeId && d.AssigneAFournisseurId == currentUserId);

            if (demande == null)
            {
                return NotFound("Demande non trouvée ou vous n'êtes pas assigné à cette demande.");
            }

            if (demande.Status != StatusDemande.EnCours && demande.Status != StatusDemande.Acceptee)
            {
                return BadRequest("Cette demande n'est pas en cours ou acceptée et ne peut pas recevoir de réponse.");
            }

            // Vérifier que le menu proposé appartient au fournisseur
            var menu = await _context.Menus
                .Include(m => m.Fournisseur)
                .FirstOrDefaultAsync(m => m.Id == reponseDto.MenuProposedId);

            if (menu == null)
            {
                return NotFound("Menu proposé non trouvé.");
            }

            if (menu.FournisseurId == null || menu.Fournisseur?.UserId != currentUserId)
            {
                return BadRequest("Vous ne pouvez proposer que vos propres menus.");
            }

            var reponse = new DemandeMenuReponse
            {
                DemandeMenuId = demandeId,
                MenuProposedId = reponseDto.MenuProposedId,
                NomMenuPropose = reponseDto.NomMenuPropose,
                DescriptionMenuPropose = reponseDto.DescriptionMenuPropose,
                PrixTotal = reponseDto.PrixTotal,
                CommentairesFournisseur = reponseDto.CommentairesFournisseur,
                DateProposition = DateTime.UtcNow
            };

            _context.DemandeMenuReponses.Add(reponse);
            await _context.SaveChangesAsync();

            var reponseDto_result = new DemandeMenuReponseDto
            {
                Id = reponse.Id,
                DemandeMenuId = reponse.DemandeMenuId,
                MenuProposedId = reponse.MenuProposedId,
                NomMenuPropose = reponse.NomMenuPropose,
                DescriptionMenuPropose = reponse.DescriptionMenuPropose,
                PrixTotal = reponse.PrixTotal,
                CommentairesFournisseur = reponse.CommentairesFournisseur,
                IsAcceptedByAdmin = reponse.IsAcceptedByAdmin,
                DateProposition = reponse.DateProposition,
                DateAcceptation = reponse.DateAcceptation,
                CommentairesAcceptation = reponse.CommentairesAcceptation
            };

            return CreatedAtAction(nameof(GetFournisseur), new { id = currentUserId }, reponseDto_result);
        }


        private bool FournisseurExists(int id)
        {
            return _context.Fournisseurs.Any(e => e.Id == id);
        }
    }
}
