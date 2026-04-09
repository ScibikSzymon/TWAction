using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TWAction.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMainActionSettingsActionDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ActionDate",
                table: "MainActionSettings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionDate",
                table: "MainActionSettings");
        }
    }
}
