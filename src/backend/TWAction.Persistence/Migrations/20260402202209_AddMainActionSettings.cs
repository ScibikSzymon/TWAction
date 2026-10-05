using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TWAction.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMainActionSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MainActionSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinDepartureTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SkipNightSendings = table.Column<bool>(type: "boolean", nullable: false),
                    MaxNobleDistance = table.Column<long>(type: "bigint", nullable: false),
                    OffSettings_MinOffUnits = table.Column<long>(type: "bigint", nullable: false),
                    OffSettings_MinDistanceFromFront = table.Column<long>(type: "bigint", nullable: false),
                    CatasSettings_MinCatasNumber = table.Column<long>(type: "bigint", nullable: false),
                    CatasSettings_MinDistanceFromFront = table.Column<long>(type: "bigint", nullable: false),
                    CatasSettings_MaxOffUnits = table.Column<long>(type: "bigint", nullable: false),
                    FakeOffSettings_MinOffUnits = table.Column<long>(type: "bigint", nullable: false),
                    FakeOffSettings_MinDistanceFromFront = table.Column<long>(type: "bigint", nullable: false),
                    FakeDeffSettings_MaxOffUnits = table.Column<long>(type: "bigint", nullable: false),
                    FakeDeffSettings_MinDistanceFromFront = table.Column<long>(type: "bigint", nullable: false),
                    NobleSettings_MinDistanceFromFront = table.Column<long>(type: "bigint", nullable: false),
                    NobleSettings_MinOffUnitsForOffNoble = table.Column<long>(type: "bigint", nullable: false),
                    NobleSettings_MinOffUnitsForFakeOffNoble = table.Column<long>(type: "bigint", nullable: false),
                    NobleSettings_MaxOffUnitsForDefNoble = table.Column<long>(type: "bigint", nullable: false),
                    NobleSettings_MinDeffUnitsForDefNoble = table.Column<long>(type: "bigint", nullable: false),
                    PlayerNobleBudgets = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainActionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainActionSettings_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainActionSettings_ScheduleId",
                table: "MainActionSettings",
                column: "ScheduleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MainActionSettings");
        }
    }
}
