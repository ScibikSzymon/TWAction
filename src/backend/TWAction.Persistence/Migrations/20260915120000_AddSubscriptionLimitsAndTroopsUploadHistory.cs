using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TWAction.Persistence.Migrations;

[DbContext(typeof(TWActionDbContext))]
[Migration("20260915120000_AddSubscriptionLimitsAndTroopsUploadHistory")]
public sealed class AddSubscriptionLimitsAndTroopsUploadHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("SubscriptionTier", "Users", type: "text", nullable: false, defaultValue: "Free");
        migrationBuilder.AddColumn<int>("ScheduleLimitOverride", "Users", type: "integer", nullable: true);
        migrationBuilder.AddColumn<int>("TemplateLimitOverride", "Users", type: "integer", nullable: true);
        migrationBuilder.AddColumn<int>("TroopsUploadLimitOverride", "Users", type: "integer", nullable: true);

        migrationBuilder.CreateTable(
            name: "TroopsUploads",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TroopsUploads", x => x.Id);
                table.ForeignKey("FK_TroopsUploads_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_TroopsUploads_Schedules_ScheduleId", x => x.ScheduleId, "Schedules", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_TroopsUploads_UserId", "TroopsUploads", "UserId");
        migrationBuilder.CreateIndex("IX_TroopsUploads_ScheduleId_UploadedAt", "TroopsUploads", new[] { "ScheduleId", "UploadedAt" });
        migrationBuilder.Sql("""
            INSERT INTO "TroopsUploads" ("Id", "UserId", "ScheduleId", "UploadedAt")
            SELECT t."Id", s."UserGuid", t."ScheduleId", t."UpdatedAt"
            FROM "TroopsStates" t
            JOIN "Schedules" s ON s."Id" = t."ScheduleId"
            WHERE t."UpdatedAt" > NOW() - INTERVAL '12 hours';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("TroopsUploads");
        migrationBuilder.DropColumn("SubscriptionTier", "Users");
        migrationBuilder.DropColumn("ScheduleLimitOverride", "Users");
        migrationBuilder.DropColumn("TemplateLimitOverride", "Users");
        migrationBuilder.DropColumn("TroopsUploadLimitOverride", "Users");
    }
}
