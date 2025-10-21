using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BicTechBack.Migrations
{
    /// <inheritdoc />
    public partial class AddPaisToMarca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaisId",
                table: "Marcas",
                type: "int",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "Paises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paises", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Marcas_PaisId",
                table: "Marcas",
                column: "PaisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcas_Paises_PaisId",
                table: "Marcas",
                column: "PaisId",
                principalTable: "Paises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marcas_Paises_PaisId",
                table: "Marcas");

            migrationBuilder.DropTable(
                name: "Paises");

            migrationBuilder.DropIndex(
                name: "IX_Marcas_PaisId",
                table: "Marcas");

            migrationBuilder.DropColumn(
                name: "PaisId",
                table: "Marcas");
        }
    }
}
