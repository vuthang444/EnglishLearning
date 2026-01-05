using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServiceLib.Migrations
{
    /// <inheritdoc />
    public partial class AddListeningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Lessons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DefaultSpeed",
                table: "Lessons",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "HideTranscript",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PlayLimit",
                table: "Lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transcript",
                table: "Lessons",
                type: "nvarchar(max)",
                maxLength: 20000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timestamp",
                table: "Exercises",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 20, 15, 36, 320, DateTimeKind.Utc).AddTicks(2520));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 20, 15, 36, 320, DateTimeKind.Utc).AddTicks(2522));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 20, 15, 36, 320, DateTimeKind.Utc).AddTicks(2524));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 20, 15, 36, 320, DateTimeKind.Utc).AddTicks(2525));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "DefaultSpeed",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "HideTranscript",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "PlayLimit",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Transcript",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Exercises");

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 17, 0, 45, 972, DateTimeKind.Utc).AddTicks(5271));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 17, 0, 45, 972, DateTimeKind.Utc).AddTicks(5273));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 17, 0, 45, 972, DateTimeKind.Utc).AddTicks(5274));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 17, 0, 45, 972, DateTimeKind.Utc).AddTicks(5275));
        }
    }
}
