using Microsoft.EntityFrameworkCore.Migrations;

namespace VaquinhaAnimal.Infrastructure.Data.Migrations
{
    public partial class inserindoUrlAdocao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlAdocao",
                table: "Adocoes",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlAdocao",
                table: "Adocoes");
        }
    }
}
