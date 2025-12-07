using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GsC.API.Models;

namespace GsC.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for custom entities
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        
        // DbSets for GSC entities
        public DbSet<Vol> Vols { get; set; }
        public DbSet<PlanHebergement> PlansHebergement { get; set; }
        public DbSet<PlanHebergementArticle> PlanHebergementArticles { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuPlanHebergement> MenusPlanHebergement { get; set; }
        public DbSet<BonCommandePrevisionnel> BonsCommandePrevisionnels { get; set; }
        public DbSet<BonCommandePrevisionnelLigne> BonCommandePrevisionnelLignes { get; set; }
        public DbSet<BonLivraison> BonsLivraison { get; set; }
        public DbSet<BonLivraisonLigne> BonLivraisonLignes { get; set; }
        public DbSet<Ecart> Ecarts { get; set; }
        public DbSet<BoiteMedicale> BoitesMedicales { get; set; }
        public DbSet<BoiteMedicaleItem> BoiteMedicaleItems { get; set; }
        public DbSet<VolBoiteMedicale> VolBoitesMedicales { get; set; }
        public DbSet<DossierVol> DossiersVol { get; set; }
        public DbSet<DossierVolDocument> DossierVolDocuments { get; set; }
        public DbSet<RapportBudgetaire> RapportsBudgetaires { get; set; }
        public DbSet<RapportBudgetaireDetail> RapportBudgetaireDetails { get; set; }
        
        // Nouveaux modèles pour le système de demandes
        public DbSet<Fournisseur> Fournisseurs { get; set; }
        public DbSet<DemandeMenu> DemandesMenu { get; set; }
        public DbSet<DemandePlat> DemandePlats { get; set; }
        public DbSet<DemandeMenuReponse> DemandeMenuReponses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables with custom names
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<UserRole>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
            });

            // Configure Role entity
            builder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure Permission entity
            builder.Entity<Permission>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Module).HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure RolePermission many-to-many relationship
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });
                
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserRole relationship
            builder.Entity<UserRole>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure GSC entities
            ConfigureGscEntities(builder);
        }

        private void ConfigureGscEntities(ModelBuilder builder)
        {
            // Configure Vol
            builder.Entity<Vol>(entity =>
            {
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Aircraft).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Origin).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Destination).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Zone).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Season).HasMaxLength(20);
                entity.HasIndex(e => e.FlightNumber);
                entity.HasIndex(e => e.FlightDate);
            });

            // Configure PlanHebergement
            builder.Entity<PlanHebergement>(entity =>
            {
                entity.HasOne(e => e.Vol)
                    .WithOne(v => v.PlanHebergement)
                    .HasForeignKey<PlanHebergement>(e => e.VolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PlanHebergementArticle
            builder.Entity<PlanHebergementArticle>(entity =>
            {
                entity.HasOne(e => e.PlanHebergement)
                    .WithMany(p => p.PlanHebergementArticles)
                    .HasForeignKey(e => e.PlanHebergementId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Article)
                    .WithMany(a => a.PlanHebergementArticles)
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Article
            builder.Entity<Article>(entity =>
            {
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Configure Menu
            builder.Entity<Menu>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TypePassager).IsRequired().HasMaxLength(50);
            });

            // Configure MenuItem
            builder.Entity<MenuItem>(entity =>
            {
                entity.HasOne(e => e.Menu)
                    .WithMany(m => m.MenuItems)
                    .HasForeignKey(e => e.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Article)
                    .WithMany(a => a.MenuItems)
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MenuPlanHebergement
            builder.Entity<MenuPlanHebergement>(entity =>
            {
                entity.HasOne(e => e.Menu)
                    .WithMany(m => m.MenusPlanHebergement)
                    .HasForeignKey(e => e.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PlanHebergement)
                    .WithMany(p => p.MenusPlanHebergement)
                    .HasForeignKey(e => e.PlanHebergementId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BonCommandePrevisionnel
            builder.Entity<BonCommandePrevisionnel>(entity =>
            {
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Numero).IsUnique();
                
                entity.HasOne(e => e.Vol)
                    .WithMany(v => v.BonsCommandePrevisionnels)
                    .HasForeignKey(e => e.VolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BonCommandePrevisionnelLigne
            builder.Entity<BonCommandePrevisionnelLigne>(entity =>
            {
                entity.HasOne(e => e.BonCommandePrevisionnel)
                    .WithMany(b => b.Lignes)
                    .HasForeignKey(e => e.BonCommandePrevisionnelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Article)
                    .WithMany(a => a.BonCommandePrevisionnelLignes)
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure BonLivraison
            builder.Entity<BonLivraison>(entity =>
            {
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Fournisseur).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Numero).IsUnique();
                
                entity.HasOne(e => e.Vol)
                    .WithMany(v => v.BonsLivraison)
                    .HasForeignKey(e => e.VolId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BonCommandePrevisionnel)
                    .WithMany()
                    .HasForeignKey(e => e.BonCommandePrevisionnelId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure BonLivraisonLigne
            builder.Entity<BonLivraisonLigne>(entity =>
            {
                entity.HasOne(e => e.BonLivraison)
                    .WithMany(b => b.Lignes)
                    .HasForeignKey(e => e.BonLivraisonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Article)
                    .WithMany(a => a.BonLivraisonLignes)
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Ecart
            builder.Entity<Ecart>(entity =>
            {
                entity.HasOne(e => e.Vol)
                    .WithMany()
                    .HasForeignKey(e => e.VolId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Article)
                    .WithMany(a => a.Ecarts)
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BonCommandePrevisionnel)
                    .WithMany(b => b.Ecarts)
                    .HasForeignKey(e => e.BonCommandePrevisionnelId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.BonLivraison)
                    .WithMany(b => b.Ecarts)
                    .HasForeignKey(e => e.BonLivraisonId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure BoiteMedicale
            builder.Entity<BoiteMedicale>(entity =>
            {
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Numero).IsUnique();
            });

            // Configure BoiteMedicaleItem
            builder.Entity<BoiteMedicaleItem>(entity =>
            {
                entity.HasOne(e => e.BoiteMedicale)
                    .WithMany(b => b.Items)
                    .HasForeignKey(e => e.BoiteMedicaleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure VolBoiteMedicale
            builder.Entity<VolBoiteMedicale>(entity =>
            {
                entity.HasOne(e => e.Vol)
                    .WithMany(v => v.VolBoitesMedicales)
                    .HasForeignKey(e => e.VolId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.BoiteMedicale)
                    .WithMany(b => b.VolBoitesMedicales)
                    .HasForeignKey(e => e.BoiteMedicaleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DossierVol
            builder.Entity<DossierVol>(entity =>
            {
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Numero).IsUnique();
                
                entity.HasOne(e => e.Vol)
                    .WithOne(v => v.DossierVol)
                    .HasForeignKey<DossierVol>(e => e.VolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DossierVolDocument
            builder.Entity<DossierVolDocument>(entity =>
            {
                entity.HasOne(e => e.DossierVol)
                    .WithMany(d => d.Documents)
                    .HasForeignKey(e => e.DossierVolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RapportBudgetaire
            builder.Entity<RapportBudgetaire>(entity =>
            {
                entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
            });

            // Configure RapportBudgetaireDetail
            builder.Entity<RapportBudgetaireDetail>(entity =>
            {
                entity.HasOne(e => e.RapportBudgetaire)
                    .WithMany(r => r.Details)
                    .HasForeignKey(e => e.RapportBudgetaireId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration des nouvelles entités
            ConfigureDemandeEntities(builder);
        }

        private void ConfigureDemandeEntities(ModelBuilder builder)
        {
            // Configuration Fournisseur
            builder.Entity<Fournisseur>(entity =>
            {
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.ContactEmail).HasMaxLength(100);
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.Siret).HasMaxLength(50);
                entity.Property(e => e.NumeroTVA).HasMaxLength(50);
                entity.Property(e => e.Specialites).HasMaxLength(500);
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<Fournisseur>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration DemandeMenu
            builder.Entity<DemandeMenu>(entity =>
            {
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CommentairesAdmin).HasMaxLength(500);
                entity.Property(e => e.CommentairesFournisseur).HasMaxLength(500);
                entity.HasIndex(e => e.Numero).IsUnique();

                entity.HasOne(e => e.DemandeParUser)
                    .WithMany()
                    .HasForeignKey(e => e.DemandeParUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssigneAFournisseur)
                    .WithMany()
                    .HasForeignKey(e => e.AssigneAFournisseurId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuration DemandePlat
            builder.Entity<DemandePlat>(entity =>
            {
                entity.Property(e => e.NomPlatSouhaite).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DescriptionSouhaitee).HasMaxLength(500);
                entity.Property(e => e.UniteSouhaitee).HasMaxLength(50);
                entity.Property(e => e.SpecificationsSpeciales).HasMaxLength(200);
                entity.Property(e => e.CommentairesFournisseur).HasMaxLength(500);

                entity.HasOne(e => e.DemandeMenu)
                    .WithMany(d => d.DemandePlats)
                    .HasForeignKey(e => e.DemandeMenuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ArticleProposed)
                    .WithMany(a => a.DemandePlats)
                    .HasForeignKey(e => e.ArticleProposedId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuration DemandeMenuReponse
            builder.Entity<DemandeMenuReponse>(entity =>
            {
                entity.Property(e => e.NomMenuPropose).HasMaxLength(200);
                entity.Property(e => e.DescriptionMenuPropose).HasMaxLength(1000);
                entity.Property(e => e.CommentairesFournisseur).HasMaxLength(500);
                entity.Property(e => e.CommentairesAcceptation).HasMaxLength(500);

                entity.HasOne(e => e.DemandeMenu)
                    .WithMany(d => d.Reponses)
                    .HasForeignKey(e => e.DemandeMenuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.MenuProposed)
                    .WithMany(m => m.DemandeMenuReponses)
                    .HasForeignKey(e => e.MenuProposedId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Mise à jour des relations Article et Menu avec Fournisseur
            builder.Entity<Article>(entity =>
            {
                entity.HasOne(e => e.Fournisseur)
                    .WithMany(f => f.Articles)
                    .HasForeignKey(e => e.FournisseurId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Menu>(entity =>
            {
                entity.HasOne(e => e.Fournisseur)
                    .WithMany(f => f.Menus)
                    .HasForeignKey(e => e.FournisseurId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}