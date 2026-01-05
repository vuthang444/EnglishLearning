using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServiceLib.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingContentToLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReadingContent",
                table: "Lessons",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReadingLevel",
                table: "Lessons",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 16, 16, 3, 930, DateTimeKind.Utc).AddTicks(4120));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 16, 16, 3, 930, DateTimeKind.Utc).AddTicks(4124));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 16, 16, 3, 930, DateTimeKind.Utc).AddTicks(4127));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 16, 16, 3, 930, DateTimeKind.Utc).AddTicks(4129));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadingContent",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ReadingLevel",
                table: "Lessons");

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 14, 40, 33, 517, DateTimeKind.Utc).AddTicks(7816));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 14, 40, 33, 517, DateTimeKind.Utc).AddTicks(7818));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 14, 40, 33, 517, DateTimeKind.Utc).AddTicks(7819));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 14, 40, 33, 517, DateTimeKind.Utc).AddTicks(7820));
        }
    }
}
