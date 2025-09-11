using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GsC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGscEntitiesFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoitesMedicales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateExpiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DerniereMaintenance = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProchaineMaintenance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsableMaintenance = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoitesMedicales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TypePassager = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Zone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RapportsBudgetaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeRapport = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateGeneration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GenerePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NombreVolsTotal = table.Column<int>(type: "int", nullable: false),
                    MontantPrevu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantReel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EcartMontant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PourcentageEcart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoutRepas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoutBoissons = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoutConsommables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoutSemiConsommables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoutMaterielDivers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CheminFichier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RapportsBudgetaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FlightDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Aircraft = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstimatedPassengers = table.Column<int>(type: "int", nullable: false),
                    ActualPassengers = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoiteMedicaleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoiteMedicaleId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantite = table.Column<int>(type: "int", nullable: false),
                    Unite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fabricant = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumeroLot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoiteMedicaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoiteMedicaleItems_BoitesMedicales_BoiteMedicaleId",
                        column: x => x.BoiteMedicaleId,
                        principalTable: "BoitesMedicales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TypePassager = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItems_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RapportBudgetaireDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RapportBudgetaireId = table.Column<int>(type: "int", nullable: false),
                    Categorie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Libelle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MontantPrevu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantReel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ecart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PourcentageEcart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantite = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RapportBudgetaireDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RapportBudgetaireDetails_RapportsBudgetaires_RapportBudgetaireId",
                        column: x => x.RapportBudgetaireId,
                        principalTable: "RapportsBudgetaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonsCommandePrevisionnels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    DateCommande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Fournisseur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MontantTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsCommandePrevisionnels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonsCommandePrevisionnels_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DossiersVol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Resume = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Commentaires = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CoutTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NombreEcarts = table.Column<int>(type: "int", nullable: false),
                    MontantEcarts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossiersVol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DossiersVol_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlansHebergement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AircraftType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FlightDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlansHebergement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlansHebergement_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolBoitesMedicales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    BoiteMedicaleId = table.Column<int>(type: "int", nullable: false),
                    DateAssignation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Commentaires = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolBoitesMedicales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolBoitesMedicales_BoitesMedicales_BoiteMedicaleId",
                        column: x => x.BoiteMedicaleId,
                        principalTable: "BoitesMedicales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VolBoitesMedicales_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonCommandePrevisionnelLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BonCommandePrevisionnelId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteCommandee = table.Column<int>(type: "int", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantLigne = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonCommandePrevisionnelLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonCommandePrevisionnelLignes_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonCommandePrevisionnelLignes_BonsCommandePrevisionnels_BonCommandePrevisionnelId",
                        column: x => x.BonCommandePrevisionnelId,
                        principalTable: "BonsCommandePrevisionnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonsLivraison",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    BonCommandePrevisionnelId = table.Column<int>(type: "int", nullable: true),
                    DateLivraison = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Fournisseur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Livreur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Commentaires = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MontantTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ValidationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsLivraison", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonsLivraison_BonsCommandePrevisionnels_BonCommandePrevisionnelId",
                        column: x => x.BonCommandePrevisionnelId,
                        principalTable: "BonsCommandePrevisionnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BonsLivraison_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DossierVolDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DossierVolId = table.Column<int>(type: "int", nullable: false),
                    NomDocument = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeDocument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CheminFichier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FormatFichier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TailleFichier = table.Column<long>(type: "bigint", nullable: false),
                    DateAjout = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AjoutePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierVolDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DossierVolDocuments_DossiersVol_DossierVolId",
                        column: x => x.DossierVolId,
                        principalTable: "DossiersVol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenusPlanHebergement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    PlanHebergementId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenusPlanHebergement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenusPlanHebergement_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenusPlanHebergement_PlansHebergement_PlanHebergementId",
                        column: x => x.PlanHebergementId,
                        principalTable: "PlansHebergement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanHebergementArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanHebergementId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteStandard = table.Column<int>(type: "int", nullable: false),
                    TypePassager = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanHebergementArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanHebergementArticles_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanHebergementArticles_PlansHebergement_PlanHebergementId",
                        column: x => x.PlanHebergementId,
                        principalTable: "PlansHebergement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonLivraisonLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BonLivraisonId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteLivree = table.Column<int>(type: "int", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantLigne = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonLivraisonLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonLivraisonLignes_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonLivraisonLignes_BonsLivraison_BonLivraisonId",
                        column: x => x.BonLivraisonId,
                        principalTable: "BonsLivraison",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ecarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    BonCommandePrevisionnelId = table.Column<int>(type: "int", nullable: true),
                    BonLivraisonId = table.Column<int>(type: "int", nullable: true),
                    TypeEcart = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QuantiteCommandee = table.Column<int>(type: "int", nullable: false),
                    QuantiteLivree = table.Column<int>(type: "int", nullable: false),
                    EcartQuantite = table.Column<int>(type: "int", nullable: false),
                    PrixCommande = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrixLivraison = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EcartMontant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActionCorrective = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResponsableTraitement = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateDetection = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateResolution = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ecarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ecarts_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ecarts_BonsCommandePrevisionnels_BonCommandePrevisionnelId",
                        column: x => x.BonCommandePrevisionnelId,
                        principalTable: "BonsCommandePrevisionnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ecarts_BonsLivraison_BonLivraisonId",
                        column: x => x.BonLivraisonId,
                        principalTable: "BonsLivraison",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ecarts_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Code",
                table: "Articles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoiteMedicaleItems_BoiteMedicaleId",
                table: "BoiteMedicaleItems",
                column: "BoiteMedicaleId");

            migrationBuilder.CreateIndex(
                name: "IX_BoitesMedicales_Numero",
                table: "BoitesMedicales",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonCommandePrevisionnelLignes_ArticleId",
                table: "BonCommandePrevisionnelLignes",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_BonCommandePrevisionnelLignes_BonCommandePrevisionnelId",
                table: "BonCommandePrevisionnelLignes",
                column: "BonCommandePrevisionnelId");

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraisonLignes_ArticleId",
                table: "BonLivraisonLignes",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraisonLignes_BonLivraisonId",
                table: "BonLivraisonLignes",
                column: "BonLivraisonId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsCommandePrevisionnels_Numero",
                table: "BonsCommandePrevisionnels",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonsCommandePrevisionnels_VolId",
                table: "BonsCommandePrevisionnels",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_BonCommandePrevisionnelId",
                table: "BonsLivraison",
                column: "BonCommandePrevisionnelId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_Numero",
                table: "BonsLivraison",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_VolId",
                table: "BonsLivraison",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_DossiersVol_Numero",
                table: "DossiersVol",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DossiersVol_VolId",
                table: "DossiersVol",
                column: "VolId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DossierVolDocuments_DossierVolId",
                table: "DossierVolDocuments",
                column: "DossierVolId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecarts_ArticleId",
                table: "Ecarts",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecarts_BonCommandePrevisionnelId",
                table: "Ecarts",
                column: "BonCommandePrevisionnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecarts_BonLivraisonId",
                table: "Ecarts",
                column: "BonLivraisonId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecarts_VolId",
                table: "Ecarts",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ArticleId",
                table: "MenuItems",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuId",
                table: "MenuItems",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenusPlanHebergement_MenuId",
                table: "MenusPlanHebergement",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenusPlanHebergement_PlanHebergementId",
                table: "MenusPlanHebergement",
                column: "PlanHebergementId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanHebergementArticles_ArticleId",
                table: "PlanHebergementArticles",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanHebergementArticles_PlanHebergementId",
                table: "PlanHebergementArticles",
                column: "PlanHebergementId");

            migrationBuilder.CreateIndex(
                name: "IX_PlansHebergement_VolId",
                table: "PlansHebergement",
                column: "VolId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RapportBudgetaireDetails_RapportBudgetaireId",
                table: "RapportBudgetaireDetails",
                column: "RapportBudgetaireId");

            migrationBuilder.CreateIndex(
                name: "IX_VolBoitesMedicales_BoiteMedicaleId",
                table: "VolBoitesMedicales",
                column: "BoiteMedicaleId");

            migrationBuilder.CreateIndex(
                name: "IX_VolBoitesMedicales_VolId",
                table: "VolBoitesMedicales",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_Vols_FlightDate",
                table: "Vols",
                column: "FlightDate");

            migrationBuilder.CreateIndex(
                name: "IX_Vols_FlightNumber",
                table: "Vols",
                column: "FlightNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoiteMedicaleItems");

            migrationBuilder.DropTable(
                name: "BonCommandePrevisionnelLignes");

            migrationBuilder.DropTable(
                name: "BonLivraisonLignes");

            migrationBuilder.DropTable(
                name: "DossierVolDocuments");

            migrationBuilder.DropTable(
                name: "Ecarts");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "MenusPlanHebergement");

            migrationBuilder.DropTable(
                name: "PlanHebergementArticles");

            migrationBuilder.DropTable(
                name: "RapportBudgetaireDetails");

            migrationBuilder.DropTable(
                name: "VolBoitesMedicales");

            migrationBuilder.DropTable(
                name: "DossiersVol");

            migrationBuilder.DropTable(
                name: "BonsLivraison");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "PlansHebergement");

            migrationBuilder.DropTable(
                name: "RapportsBudgetaires");

            migrationBuilder.DropTable(
                name: "BoitesMedicales");

            migrationBuilder.DropTable(
                name: "BonsCommandePrevisionnels");

            migrationBuilder.DropTable(
                name: "Vols");
        }
    }
}
