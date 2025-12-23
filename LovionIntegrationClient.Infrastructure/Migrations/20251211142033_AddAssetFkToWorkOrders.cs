using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LovionIntegrationClient.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetFkToWorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssetId",
                table: "WorkOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssetId",
                table: "WorkOrders",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Assets_AssetId",
                table: "WorkOrders",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Assets_AssetId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_AssetId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "WorkOrders");
        }
    }
}
