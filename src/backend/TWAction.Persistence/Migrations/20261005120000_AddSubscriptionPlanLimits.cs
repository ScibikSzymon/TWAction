using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TWAction.Persistence.Migrations;

[DbContext(typeof(TWActionDbContext))]
[Migration("20261005120000_AddSubscriptionPlanLimits")]
public sealed class AddSubscriptionPlanLimits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SubscriptionPlanLimits",
            columns: table => new
            {
                SubscriptionTier = table.Column<string>(type: "text", nullable: false),
                ScheduleLimit = table.Column<int>(type: "integer", nullable: true),
                TemplateLimit = table.Column<int>(type: "integer", nullable: true),
                TroopsUploadLimit = table.Column<int>(type: "integer", nullable: true),
                TroopsUploadWindowHours = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubscriptionPlanLimits", x => x.SubscriptionTier);
            });

        migrationBuilder.Sql("""
            INSERT INTO "SubscriptionPlanLimits"
                ("SubscriptionTier", "ScheduleLimit", "TemplateLimit", "TroopsUploadLimit", "TroopsUploadWindowHours")
            VALUES
                ('Free', 3, 1, 2, 12),
                ('Premium', NULL, NULL, NULL, 12);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SubscriptionPlanLimits");
    }
}
