using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TWAction.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReconnaissanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReconnaissanceSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinDepartureTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MinArrivalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MaxArrivalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MinDistanceToFront = table.Column<int>(type: "integer", nullable: false),
                    MinSpyCount = table.Column<int>(type: "integer", nullable: false),
                    MaxPopulationInSourceVillage = table.Column<int>(type: "integer", nullable: false),
                    SkipNightSendings = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconnaissanceSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconnaissanceSettings_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReconnaissanceSettings_ScheduleId",
                table: "ReconnaissanceSettings",
                column: "ScheduleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReconnaissanceSettings");
        }
    }
}
