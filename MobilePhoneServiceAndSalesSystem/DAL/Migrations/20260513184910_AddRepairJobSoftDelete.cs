using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobilePhoneServiceAndSalesSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairJobSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RepairJobs",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RepairJobs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RepairJobs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RepairJobs");
        }
    }
}
