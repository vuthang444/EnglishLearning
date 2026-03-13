using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataServiceLib.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsFreePreview = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                });


            migrationBuilder.CreateTable(
                name: "NewsVocabularies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VietnameseMeaning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Example = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsVocabularies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsVocabularies_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.CreateIndex(
                name: "IX_NewsVocabularies_NewsId",
                table: "NewsVocabularies",
                column: "NewsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsVocabularies");

            migrationBuilder.DropTable(
                name: "News");
        }
    }
}
