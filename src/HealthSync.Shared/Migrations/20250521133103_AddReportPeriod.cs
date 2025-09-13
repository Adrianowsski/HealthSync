using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthSync.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddReportPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodFrom",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodTo",
                table: "Reports",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeriodFrom",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PeriodTo",
                table: "Reports");
        }
    }
}
