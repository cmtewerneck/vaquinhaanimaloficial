using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VaquinhaAnimal.Infrastructure.Data.Migrations
{
    public partial class InserindoArtigos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artigos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Resumo = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: false),
                    EscritoPor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Html = table.Column<string>(type: "varchar(8000)", maxLength: 10000, nullable: false),
                    FotoCapa = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artigos", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artigos");
        }
    }
}
