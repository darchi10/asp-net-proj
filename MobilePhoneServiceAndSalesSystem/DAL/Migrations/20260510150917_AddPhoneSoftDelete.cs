using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobilePhoneServiceAndSalesSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Phones",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Phones",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Phones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Phones");
        }
    }
}
