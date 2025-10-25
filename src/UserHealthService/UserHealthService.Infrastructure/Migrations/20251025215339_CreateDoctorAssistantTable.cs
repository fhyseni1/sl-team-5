using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserHealthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDoctorAssistantTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DoctorAssistants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssistantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAssistants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorAssistants_Users_AssistantId",
                        column: x => x.AssistantId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorAssistants_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAssistants_AssistantId",
                table: "DoctorAssistants",
                column: "AssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAssistants_DoctorId",
                table: "DoctorAssistants",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorAssistants");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Appointments");
        }
    }
}
