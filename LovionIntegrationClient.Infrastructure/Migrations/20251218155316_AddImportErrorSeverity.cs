using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LovionIntegrationClient.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportErrorSeverity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "ImportErrors",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Severity",
                table: "ImportErrors");
        }
    }
}
