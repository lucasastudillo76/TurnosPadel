using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosPadel.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCanchaATurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cancha",
                table: "Turnos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cancha",
                table: "Turnos");
        }
    }
}
