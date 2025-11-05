using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAndPrescribedByToMedication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Medications_MedicationId1",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_MedicationId1",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "MedicationId1",
                table: "Prescriptions");

            migrationBuilder.AlterColumn<int>(
                name: "RemainingRefills",
                table: "Prescriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "Medications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescribedBy",
                table: "Medications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "PrescribedBy",
                table: "Medications");

            migrationBuilder.AlterColumn<int>(
                name: "RemainingRefills",
                table: "Prescriptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "MedicationId1",
                table: "Prescriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicationId1",
                table: "Prescriptions",
                column: "MedicationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Medications_MedicationId1",
                table: "Prescriptions",
                column: "MedicationId1",
                principalTable: "Medications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
