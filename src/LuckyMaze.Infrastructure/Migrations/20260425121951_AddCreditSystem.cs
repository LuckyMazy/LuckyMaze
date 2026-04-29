using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyMaze.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreditBalance",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "CostMultiplier",
                table: "AdminSettings",
                type: "numeric(8,4)",
                precision: 8,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditsPerUsd",
                table: "AdminSettings",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "StartingBalance",
                table: "AdminSettings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditBalance",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CostMultiplier",
                table: "AdminSettings");

            migrationBuilder.DropColumn(
                name: "CreditsPerUsd",
                table: "AdminSettings");

            migrationBuilder.DropColumn(
                name: "StartingBalance",
                table: "AdminSettings");
        }
    }
}
