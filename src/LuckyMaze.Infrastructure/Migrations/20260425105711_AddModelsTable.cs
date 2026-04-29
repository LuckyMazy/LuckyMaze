using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyMaze.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModelsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContextLength = table.Column<int>(type: "integer", nullable: false),
                    InputModalities = table.Column<List<string>>(type: "text[]", nullable: false),
                    OutputModalities = table.Column<List<string>>(type: "text[]", nullable: false),
                    PromptPricePerMillion = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    CompletionPricePerMillion = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Models_ModelId",
                table: "Models",
                column: "ModelId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Models");
        }
    }
}
