using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TWAction.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttackCommands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttackCommands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinDepartureTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MaxDepartureTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MinArrivalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MaxArrivalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceVillageId = table.Column<int>(type: "integer", nullable: false),
                    SourceX = table.Column<int>(type: "integer", nullable: false),
                    SourceY = table.Column<int>(type: "integer", nullable: false),
                    SourcePlayerId = table.Column<int>(type: "integer", nullable: false),
                    DestinationVillageId = table.Column<int>(type: "integer", nullable: false),
                    DestinationX = table.Column<int>(type: "integer", nullable: false),
                    DestinationY = table.Column<int>(type: "integer", nullable: false),
                    DestinationPlayerId = table.Column<int>(type: "integer", nullable: false),
                    CommandType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackCommands", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttackCommands_CreatedAt",
                table: "AttackCommands",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AttackCommands_ScheduleId",
                table: "AttackCommands",
                column: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttackCommands");
        }
    }
}
