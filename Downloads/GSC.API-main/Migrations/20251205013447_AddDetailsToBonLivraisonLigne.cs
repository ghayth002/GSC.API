using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GsC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailsToBonLivraisonLigne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DemandePlatId",
                table: "BonLivraisonLignes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomArticle",
                table: "BonLivraisonLignes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemandePlatId",
                table: "BonLivraisonLignes");

            migrationBuilder.DropColumn(
                name: "NomArticle",
                table: "BonLivraisonLignes");
        }
    }
}
