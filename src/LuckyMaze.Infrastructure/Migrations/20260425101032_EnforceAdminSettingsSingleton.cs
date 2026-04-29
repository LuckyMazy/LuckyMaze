using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyMaze.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAdminSettingsSingleton : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AdminSettings_Singleton",
                table: "AdminSettings",
                sql: "\"Id\" = '00000000-0000-0000-0000-000000000001'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AdminSettings_Singleton",
                table: "AdminSettings");
        }
    }
}
