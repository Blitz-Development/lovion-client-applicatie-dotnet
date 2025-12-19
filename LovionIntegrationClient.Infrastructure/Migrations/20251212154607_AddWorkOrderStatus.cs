using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LovionIntegrationClient.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WorkOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkOrders");
        }
    }
}
