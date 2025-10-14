using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosPadel.Migrations
{
    /// <inheritdoc />
    public partial class ContraseñaHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContraseñaHash",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContraseñaHash",
                table: "Usuarios");
        }
    }
}
