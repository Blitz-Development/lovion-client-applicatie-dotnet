using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LovionIntegrationClient.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportRunRetryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastRetryAtUtc",
                table: "ImportRuns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRetryAtUtc",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "ImportRuns");
        }
    }
}
