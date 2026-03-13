using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServiceLib.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedImagePathToNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturedImagePath",
                table: "News",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 7, 11, 31, 3, 219, DateTimeKind.Utc).AddTicks(4152));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 7, 11, 31, 3, 219, DateTimeKind.Utc).AddTicks(4154));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 7, 11, 31, 3, 219, DateTimeKind.Utc).AddTicks(4155));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 7, 11, 31, 3, 219, DateTimeKind.Utc).AddTicks(4157));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeaturedImagePath",
                table: "News");

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 17, 48, 17, 344, DateTimeKind.Utc).AddTicks(8491));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 17, 48, 17, 344, DateTimeKind.Utc).AddTicks(8492));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 17, 48, 17, 344, DateTimeKind.Utc).AddTicks(8494));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 17, 48, 17, 344, DateTimeKind.Utc).AddTicks(8495));
        }
    }
}
