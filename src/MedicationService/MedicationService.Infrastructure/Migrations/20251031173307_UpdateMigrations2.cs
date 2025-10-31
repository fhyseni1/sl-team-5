using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigrations2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RemainingRefills",
                table: "Prescriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingRefills",
                table: "Prescriptions");
        }
    }
}
