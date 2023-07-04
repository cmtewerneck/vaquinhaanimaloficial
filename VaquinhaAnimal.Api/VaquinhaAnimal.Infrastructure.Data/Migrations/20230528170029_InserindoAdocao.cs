using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VaquinhaAnimal.Infrastructure.Data.Migrations
{
    public partial class InserindoAdocao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adocoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomePet = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    TipoPet = table.Column<int>(type: "int", nullable: false),
                    Idade_Anos = table.Column<int>(type: "int", nullable: false),
                    Idade_Meses = table.Column<int>(type: "int", nullable: false),
                    Castrado = table.Column<bool>(type: "bit", nullable: false),
                    Abrigo = table.Column<bool>(type: "bit", nullable: false),
                    Abrigo_Nome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Empresa = table.Column<bool>(type: "bit", nullable: false),
                    Empresa_Nome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Adotado = table.Column<bool>(type: "bit", nullable: false),
                    Foto = table.Column<string>(type: "varchar(100)", nullable: true),
                    UsuarioId = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adocoes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adocoes");
        }
    }
}
