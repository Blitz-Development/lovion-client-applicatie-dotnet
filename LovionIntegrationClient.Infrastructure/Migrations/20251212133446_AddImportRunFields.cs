using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LovionIntegrationClient.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportRunFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SourceSystem",
                table: "ImportRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ImportRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportRunId",
                table: "ImportErrors",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ImportErrors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErrorType",
                table: "ImportErrors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalWorkOrderId",
                table: "ImportErrors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportErrors_ImportRunId",
                table: "ImportErrors",
                column: "ImportRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportErrors_ImportRuns_ImportRunId",
                table: "ImportErrors",
                column: "ImportRunId",
                principalTable: "ImportRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportErrors_ImportRuns_ImportRunId",
                table: "ImportErrors");

            migrationBuilder.DropIndex(
                name: "IX_ImportErrors_ImportRunId",
                table: "ImportErrors");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ImportErrors");

            migrationBuilder.DropColumn(
                name: "ErrorType",
                table: "ImportErrors");

            migrationBuilder.DropColumn(
                name: "ExternalWorkOrderId",
                table: "ImportErrors");

            migrationBuilder.AlterColumn<string>(
                name: "SourceSystem",
                table: "ImportRuns",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportRunId",
                table: "ImportErrors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");
        }
    }
}
