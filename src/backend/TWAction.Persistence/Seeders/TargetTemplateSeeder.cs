using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Templates;

namespace TWAction.Persistence.Seeders;

/// <summary>
/// Seeds the default (read-only) target templates into the database on startup.
/// Each template corresponds to an attack pattern from the base configuration CSV.
/// Seeds are skipped when default templates already exist.
/// </summary>
public sealed class TargetTemplateSeeder(TWActionDbContext context)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var alreadySeeded = await context.TargetTemplates
            .AnyAsync(t => t.IsDefault, ct);

        if (alreadySeeded)
        {
            return;
        }

        context.TargetTemplates.AddRange(BuildDefaultTemplates());
        await context.SaveChangesAsync(ct);
    }

    private static IEnumerable<TargetTemplate> BuildDefaultTemplates()
    {
        return
        [
            Build("25 OFF",
            [
                W("08:00", "09:30", 6, CT.Off),
                W("09:00", "10:30", 6, CT.Off),
                W("10:00", "11:30", 6, CT.Off),
                W("11:00", "12:30", 6, CT.Off),
                W("15:00", "16:30", 1, CT.Off),
                W("08:00", "09:30", 1, CT.FakeDefensive),
                W("09:00", "10:30", 1, CT.FakeDefensive),
                W("10:00", "11:30", 1, CT.FakeDefensive),
                W("11:00", "12:30", 1, CT.FakeDefensive),
                W("15:00", "16:30", 1, CT.FakeDefensive),
            ]),

            Build("17 OFF",
            [
                W("08:00", "09:30", 4, CT.Off),
                W("09:00", "10:30", 4, CT.Off),
                W("10:00", "11:30", 4, CT.Off),
                W("11:00", "12:30", 4, CT.Off),
                W("15:00", "16:30", 1, CT.Off),
                W("08:00", "09:30", 1, CT.FakeDefensive),
                W("09:00", "10:30", 1, CT.FakeDefensive),
                W("10:00", "11:30", 1, CT.FakeDefensive),
                W("11:00", "12:30", 1, CT.FakeDefensive),
                W("15:00", "16:30", 1, CT.FakeDefensive),
                W("08:00", "09:30", 2, CT.FakeOffensive),
                W("09:00", "10:30", 2, CT.FakeOffensive),
                W("10:00", "11:30", 2, CT.FakeOffensive),
                W("11:00", "12:30", 2, CT.FakeOffensive),
            ]),

            Build("9 OFF",
            [
                W("08:00", "09:30", 2, CT.Off),
                W("09:00", "10:30", 2, CT.Off),
                W("10:00", "11:30", 2, CT.Off),
                W("11:00", "12:30", 2, CT.Off),
                W("15:00", "16:30", 1, CT.Off),
                W("08:00", "09:30", 1, CT.FakeDefensive),
                W("09:00", "10:30", 1, CT.FakeDefensive),
                W("10:00", "11:30", 1, CT.FakeDefensive),
                W("11:00", "12:30", 1, CT.FakeDefensive),
                W("15:00", "16:30", 1, CT.FakeDefensive),
                W("08:00", "09:30", 4, CT.FakeOffensive),
                W("09:00", "10:30", 4, CT.FakeOffensive),
                W("10:00", "11:30", 4, CT.FakeOffensive),
                W("11:00", "12:30", 4, CT.FakeOffensive),
            ]),

            Build("5 OFF",
            [
                W("08:00", "09:30", 2, CT.Off),
                W("09:00", "10:30", 2, CT.Off),
                W("10:00", "11:30", 1, CT.Off),
                W("08:00", "09:30", 1, CT.FakeDefensive),
                W("09:00", "10:30", 1, CT.FakeDefensive),
                W("10:00", "11:30", 1, CT.FakeDefensive),
                W("11:00", "12:30", 1, CT.FakeDefensive),
                W("15:00", "16:30", 1, CT.FakeDefensive),
                W("08:00", "09:30", 4, CT.FakeOffensive),
                W("09:00", "10:30", 4, CT.FakeOffensive),
                W("10:00", "11:30", 6, CT.FakeOffensive),
                W("11:00", "12:30", 6, CT.FakeOffensive),
            ]),

            Build("30 Fejk pierwsza linia",
            [
                W("08:00", "09:30", 6, CT.FakeOffensive),
                W("09:00", "10:30", 6, CT.FakeOffensive),
                W("10:00", "11:30", 6, CT.FakeOffensive),
                W("11:00", "12:30", 6, CT.FakeOffensive),
                W("15:00", "16:30", 1, CT.FakeOffensive),
                W("08:00", "09:30", 1, CT.FakeDefensive),
                W("09:00", "10:30", 1, CT.FakeDefensive),
                W("10:00", "11:30", 1, CT.FakeDefensive),
                W("11:00", "12:30", 1, CT.FakeDefensive),
                W("15:00", "16:30", 1, CT.FakeDefensive),
            ]),

            Build("Burzenie",
            [
                W("08:00", "09:30", 2, CT.Off),
                W("09:30", "11:00", 6, CT.Catapults),
                W("10:00", "11:30", 6, CT.Catapults),
                W("09:30", "11:00", 1, CT.Off),
                W("09:30", "11:00", 2, CT.FakeOffensive),
                W("08:00", "09:30", 3, CT.FakeDefensive),
            ]),

            Build("Fejk Burzenie",
            [
                W("08:00", "09:30", 2, CT.FakeOffensive),
                W("09:30", "11:00", 6, CT.FakeDefensive),
                W("10:00", "11:30", 6, CT.FakeDefensive),
                W("09:30", "11:00", 3, CT.FakeOffensive),
                W("08:00", "09:30", 3, CT.FakeDefensive),
            ]),

            Build("Mocny Landing",
            [
                W("08:00", "09:30", 5, CT.Off),
                W("09:30", "11:00", 1, CT.NobleWithFullOff),
                W("09:30", "11:00", 1, CT.NobleWithHalfOff),
                W("10:30", "12:00", 2, CT.NobleWithFullOff),
                W("10:30", "12:00", 2, CT.NobleWithHalfOff),
                W("12:00", "13:30", 1, CT.NobleWithDeff),
                W("12:00", "13:30", 1, CT.NobleWith100HeavyCavalry),
                W("08:00", "09:30", 2, CT.FakeOffensive),
                W("10:00", "11:30", 2, CT.FakeOffensive),
                W("12:00", "13:30", 2, CT.FakeOffensive),
                W("08:00", "09:30", 2, CT.FakeDefensive),
                W("10:00", "11:30", 2, CT.FakeDefensive),
                W("12:00", "13:30", 2, CT.FakeDefensive),
            ]),

            Build("Średni Landing",
            [
                W("08:00", "09:30", 3, CT.Off),
                W("09:30", "11:00", 1, CT.NobleWithQuarterOffensive),
                W("09:30", "11:00", 1, CT.NobleWithHalfOff),
                W("10:30", "12:00", 1, CT.NobleWithFullOff),
                W("10:30", "12:00", 3, CT.NobleWithQuarterOffensive),
                W("12:00", "13:30", 1, CT.NobleWithDeff),
                W("12:00", "13:30", 1, CT.NobleWith100HeavyCavalry),
                W("08:00", "09:30", 4, CT.FakeOffensive),
                W("10:00", "11:30", 2, CT.FakeOffensive),
                W("12:00", "13:30", 2, CT.FakeOffensive),
                W("08:00", "09:30", 2, CT.FakeDefensive),
                W("10:00", "11:30", 2, CT.FakeDefensive),
                W("12:00", "13:30", 2, CT.FakeDefensive),
            ]),

            Build("Landing Cwiartek",
            [
                W("08:00", "09:30", 2, CT.Off),
                W("09:30", "11:00", 2, CT.NobleWithQuarterOffensive),
                W("10:30", "12:00", 1, CT.NobleWithHalfOff),
                W("10:30", "12:00", 3, CT.NobleWithQuarterOffensive),
                W("12:00", "13:30", 1, CT.NobleWithDeff),
                W("12:00", "13:30", 1, CT.NobleWith100HeavyCavalry),
                W("08:00", "09:30", 5, CT.FakeOffensive),
                W("10:00", "11:30", 2, CT.FakeOffensive),
                W("12:00", "13:30", 2, CT.FakeOffensive),
                W("08:00", "09:30", 2, CT.FakeDefensive),
            ]),

            Build("FejkLanding",
            [
                W("08:00", "09:30", 7, CT.FakeOffensive),
                W("09:30", "11:00", 2, CT.NobleWith150Axes),
                W("10:30", "12:00", 4, CT.NobleWith150Axes),
                W("12:00", "13:30", 2, CT.NobleWith100HeavyCavalry),
                W("10:00", "11:30", 2, CT.FakeOffensive),
                W("12:00", "13:30", 2, CT.FakeOffensive),
                W("08:00", "09:30", 2, CT.FakeDefensive),
                W("10:00", "11:30", 2, CT.FakeDefensive),
                W("12:00", "13:30", 2, CT.FakeDefensive),
            ]),

            Build("ŚmieciowyFejkLanding",
            [
                W("08:00", "09:30", 7, CT.FakeOffensive),
                W("09:30", "11:00", 2, CT.RandomNoble),
                W("10:30", "12:00", 4, CT.RandomNoble),
                W("12:00", "13:30", 2, CT.RandomNoble),
                W("10:00", "11:30", 2, CT.FakeOffensive),
                W("12:00", "13:30", 2, CT.FakeOffensive),
                W("08:00", "09:30", 2, CT.FakeDefensive),
                W("10:00", "11:30", 2, CT.FakeDefensive),
                W("12:00", "13:30", 2, CT.FakeDefensive),
            ]),
        ];
    }

    private static TargetTemplate Build(string name, List<TemplateWave> waves) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = null,
            Name = name,
            IsDefault = true,
            Waves = waves
        };

    private static TemplateWave W(string min, string max, int count, string type) =>
        new()
        {
            MinTime = TimeOnly.Parse(min),
            MaxTime = TimeOnly.Parse(max),
            CommandNumber = count,
            CommandType = type
        };

    // Shorthand alias for readability inside this file.
    private static class CT
    {
        public const string Off = CommandTypeConstants.Off;
        public const string FakeOffensive = CommandTypeConstants.FakeOffensive;
        public const string FakeDefensive = CommandTypeConstants.FakeDefensive;
        public const string Catapults = CommandTypeConstants.Catapults;
        public const string NobleWithDeff = CommandTypeConstants.NobleWithDeff;
        public const string NobleWithFullOff = CommandTypeConstants.NobleWithFullOff;
        public const string NobleWithHalfOff = CommandTypeConstants.NobleWithHalfOff;
        public const string NobleWithQuarterOffensive = CommandTypeConstants.NobleWithQuarterOffensive;
        public const string NobleWith150Axes = CommandTypeConstants.NobleWith150Axes;
        public const string NobleWith100HeavyCavalry = CommandTypeConstants.NobleWith100HeavyCavalry;
        public const string RandomNoble = CommandTypeConstants.RandomNoble;
    }
}
