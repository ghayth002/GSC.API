using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GsC.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeArticleIdNullableInBonLivraisonLigne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ArticleId",
                table: "BonLivraisonLignes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ArticleId",
                table: "BonLivraisonLignes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
