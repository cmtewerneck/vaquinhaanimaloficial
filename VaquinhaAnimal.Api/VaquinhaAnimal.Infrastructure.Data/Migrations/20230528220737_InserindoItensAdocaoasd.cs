using Microsoft.EntityFrameworkCore.Migrations;

namespace VaquinhaAnimal.Infrastructure.Data.Migrations
{
    public partial class InserindoItensAdocaoasd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Idade_Anos",
                table: "Adocoes");

            migrationBuilder.RenameColumn(
                name: "Idade_Meses",
                table: "Adocoes",
                newName: "FaixaEtaria");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FaixaEtaria",
                table: "Adocoes",
                newName: "Idade_Meses");

            migrationBuilder.AddColumn<int>(
                name: "Idade_Anos",
                table: "Adocoes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
