using Microsoft.EntityFrameworkCore.Migrations;

namespace VaquinhaAnimal.Infrastructure.Data.Migrations
{
    public partial class InserindoItensAdocaoasdasd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Abrigo",
                table: "Adocoes");

            migrationBuilder.DropColumn(
                name: "Empresa",
                table: "Adocoes");

            migrationBuilder.AddColumn<string>(
                name: "Particular_Nome",
                table: "Adocoes",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoAnunciante",
                table: "Adocoes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Particular_Nome",
                table: "Adocoes");

            migrationBuilder.DropColumn(
                name: "TipoAnunciante",
                table: "Adocoes");

            migrationBuilder.AddColumn<bool>(
                name: "Abrigo",
                table: "Adocoes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Empresa",
                table: "Adocoes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
