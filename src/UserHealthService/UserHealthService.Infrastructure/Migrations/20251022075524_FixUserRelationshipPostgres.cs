using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserHealthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelationshipPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationships_Users_UserId",
                table: "UserRelationships");

            migrationBuilder.DropIndex(
                name: "IX_UserRelationships_UserId",
                table: "UserRelationships");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserRelationships",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserRelationships",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "CanViewHealthData",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "CanScheduleAppointments",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "CanManageMedications",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_UserId_RelatedUserId_RelationshipType",
                table: "UserRelationships",
                columns: new[] { "UserId", "RelatedUserId", "RelationshipType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationships_Users_UserId",
                table: "UserRelationships",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationships_Users_UserId",
                table: "UserRelationships");

            migrationBuilder.DropIndex(
                name: "IX_UserRelationships_UserId_RelatedUserId_RelationshipType",
                table: "UserRelationships");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserRelationships",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserRelationships",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<bool>(
                name: "CanViewHealthData",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "CanScheduleAppointments",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "CanManageMedications",
                table: "UserRelationships",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_UserId",
                table: "UserRelationships",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationships_Users_UserId",
                table: "UserRelationships",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
