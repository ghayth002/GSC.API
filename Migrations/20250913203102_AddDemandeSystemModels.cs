using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GsC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDemandeSystemModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FournisseurId",
                table: "Menus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FournisseurId",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DemandesMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateDemande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateLimite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateReponse = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DemandeParUserId = table.Column<int>(type: "int", nullable: false),
                    AssigneAFournisseurId = table.Column<int>(type: "int", nullable: true),
                    CommentairesAdmin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CommentairesFournisseur = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandesMenu_Users_AssigneAFournisseurId",
                        column: x => x.AssigneAFournisseurId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DemandesMenu_Users_DemandeParUserId",
                        column: x => x.DemandeParUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Siret = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroTVA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Specialites = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fournisseurs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandeMenuReponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemandeMenuId = table.Column<int>(type: "int", nullable: false),
                    MenuProposedId = table.Column<int>(type: "int", nullable: false),
                    NomMenuPropose = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DescriptionMenuPropose = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrixTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CommentairesFournisseur = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAcceptedByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    DateProposition = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateAcceptation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommentairesAcceptation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandeMenuReponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandeMenuReponses_DemandesMenu_DemandeMenuId",
                        column: x => x.DemandeMenuId,
                        principalTable: "DemandesMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandeMenuReponses_Menus_MenuProposedId",
                        column: x => x.MenuProposedId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandePlats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemandeMenuId = table.Column<int>(type: "int", nullable: false),
                    NomPlatSouhaite = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionSouhaitee = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TypePlat = table.Column<int>(type: "int", nullable: false),
                    UniteSouhaitee = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrixMaximal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    QuantiteEstimee = table.Column<int>(type: "int", nullable: true),
                    SpecificationsSpeciales = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsObligatoire = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ArticleProposedId = table.Column<int>(type: "int", nullable: true),
                    CommentairesFournisseur = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandePlats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandePlats_Articles_ArticleProposedId",
                        column: x => x.ArticleProposedId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DemandePlats_DemandesMenu_DemandeMenuId",
                        column: x => x.DemandeMenuId,
                        principalTable: "DemandesMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_FournisseurId",
                table: "Menus",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_FournisseurId",
                table: "Articles",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandeMenuReponses_DemandeMenuId",
                table: "DemandeMenuReponses",
                column: "DemandeMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandeMenuReponses_MenuProposedId",
                table: "DemandeMenuReponses",
                column: "MenuProposedId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandePlats_ArticleProposedId",
                table: "DemandePlats",
                column: "ArticleProposedId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandePlats_DemandeMenuId",
                table: "DemandePlats",
                column: "DemandeMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesMenu_AssigneAFournisseurId",
                table: "DemandesMenu",
                column: "AssigneAFournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesMenu_DemandeParUserId",
                table: "DemandesMenu",
                column: "DemandeParUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesMenu_Numero",
                table: "DemandesMenu",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseurs_UserId",
                table: "Fournisseurs",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Fournisseurs_FournisseurId",
                table: "Articles",
                column: "FournisseurId",
                principalTable: "Fournisseurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Fournisseurs_FournisseurId",
                table: "Menus",
                column: "FournisseurId",
                principalTable: "Fournisseurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Fournisseurs_FournisseurId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Fournisseurs_FournisseurId",
                table: "Menus");

            migrationBuilder.DropTable(
                name: "DemandeMenuReponses");

            migrationBuilder.DropTable(
                name: "DemandePlats");

            migrationBuilder.DropTable(
                name: "Fournisseurs");

            migrationBuilder.DropTable(
                name: "DemandesMenu");

            migrationBuilder.DropIndex(
                name: "IX_Menus_FournisseurId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Articles_FournisseurId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "FournisseurId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "FournisseurId",
                table: "Articles");
        }
    }
}
