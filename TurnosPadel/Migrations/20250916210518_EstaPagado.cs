using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosPadel.Migrations
{
    /// <inheritdoc />
    public partial class EstaPagado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstaPagado",
                table: "Turnos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaPagado",
                table: "Turnos");
        }
    }
}
