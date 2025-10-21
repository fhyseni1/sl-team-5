using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserHealthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SymptomLog_Users_UserId",
                table: "SymptomLog");

            migrationBuilder.DropTable(
                name: "DrugInteraction");

            migrationBuilder.DropTable(
                name: "MedicationDose");

            migrationBuilder.DropTable(
                name: "MedicationReminder");

            migrationBuilder.DropTable(
                name: "Prescription");

            migrationBuilder.DropTable(
                name: "MedicationSchedule");

            migrationBuilder.DropTable(
                name: "Medication");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SymptomLog",
                table: "SymptomLog");

            migrationBuilder.RenameTable(
                name: "SymptomLog",
                newName: "SymptomLogs");

            migrationBuilder.RenameIndex(
                name: "IX_SymptomLog_UserId",
                table: "SymptomLogs",
                newName: "IX_SymptomLogs_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Users",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipType",
                table: "UserRelationships",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "HealthMetrics",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Allergies",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SymptomLogs",
                table: "SymptomLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SymptomLogs_Users_UserId",
                table: "SymptomLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SymptomLogs_Users_UserId",
                table: "SymptomLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SymptomLogs",
                table: "SymptomLogs");

            migrationBuilder.RenameTable(
                name: "SymptomLogs",
                newName: "SymptomLog");

            migrationBuilder.RenameIndex(
                name: "IX_SymptomLogs_UserId",
                table: "SymptomLog",
                newName: "IX_SymptomLog_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Users",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipType",
                table: "UserRelationships",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "HealthMetrics",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Allergies",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SymptomLog",
                table: "SymptomLog",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Medication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Dosage = table.Column<decimal>(type: "numeric", nullable: false),
                    DosageUnit = table.Column<int>(type: "integer", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenericName = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    NDCCode = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    QRCode = table.Column<string>(type: "text", nullable: true),
                    ScannedImageUrl = table.Column<string>(type: "text", nullable: true),
                    ScanningMethod = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medication_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugInteraction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalEffect = table.Column<string>(type: "text", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InteractingDrugName = table.Column<string>(type: "text", nullable: false),
                    InteractionDescription = table.Column<string>(type: "text", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    ManagementRecommendation = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugInteraction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugInteraction_Medication_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationDose",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualDosage = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsMissed = table.Column<bool>(type: "boolean", nullable: false),
                    IsTaken = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TakenTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationDose", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationDose_Medication_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationSchedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomFrequencyHours = table.Column<int>(type: "integer", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TimeOfDay = table.Column<TimeSpan>(type: "interval", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationSchedule_Medication_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    PharmacyContact = table.Column<string>(type: "text", nullable: false),
                    PharmacyName = table.Column<string>(type: "text", nullable: false),
                    PrescriberContact = table.Column<string>(type: "text", nullable: false),
                    PrescriberName = table.Column<string>(type: "text", nullable: false),
                    PrescriptionNumber = table.Column<string>(type: "text", nullable: false),
                    RemainingRefills = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescription_Medication_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationReminder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    NotificationChannel = table.Column<string>(type: "text", nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SnoozeCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationReminder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationReminder_MedicationSchedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "MedicationSchedule",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicationReminder_Medication_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugInteraction_MedicationId",
                table: "DrugInteraction",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Medication_UserId",
                table: "Medication",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDose_MedicationId",
                table: "MedicationDose",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationReminder_MedicationId",
                table: "MedicationReminder",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationReminder_ScheduleId",
                table: "MedicationReminder",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationSchedule_MedicationId",
                table: "MedicationSchedule",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_MedicationId",
                table: "Prescription",
                column: "MedicationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SymptomLog_Users_UserId",
                table: "SymptomLog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
