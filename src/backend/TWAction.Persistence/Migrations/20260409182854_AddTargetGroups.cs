using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TWAction.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TargetGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VillageCoordinates = table.Column<string>(type: "jsonb", nullable: false),
                    Waves = table.Column<string>(type: "jsonb", nullable: false),
                    BaseTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseTemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetGroups_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TargetGroups_ScheduleId",
                table: "TargetGroups",
                column: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetGroups");
        }
    }
}
