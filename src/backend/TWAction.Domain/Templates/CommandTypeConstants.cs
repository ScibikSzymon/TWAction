namespace TWAction.Domain.Templates;

/// <summary>
/// Valid command type identifiers shared with the ActionGenerator service.
/// Values match the ActionGenerator.Domain CommandType enum string names.
/// </summary>
public static class CommandTypeConstants
{
    public const string Off = "Off";
    public const string FakeOffensive = "FakeOffensive";
    public const string FakeDefensive = "FakeDefensive";
    public const string Catapults = "Catapults";
    public const string NobleWithDeff = "NobleWithDeff";
    public const string NobleWithFullOff = "NobleWithFullOff";
    public const string NobleWithHalfOff = "NobleWithHalfOff";
    public const string NobleWithQuarterOffensive = "NobleWithQuarterOffensive";
    public const string NobleWith150Axes = "NobleWith150Axes";
    public const string NobleWith100HeavyCavalry = "NobleWith100HeavyCavalry";
    public const string RandomNoble = "RandomNoble";

    /// <summary>All valid command type values for validation.</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Off, FakeOffensive, FakeDefensive, Catapults,
        NobleWithDeff, NobleWithFullOff, NobleWithHalfOff, NobleWithQuarterOffensive,
        NobleWith150Axes, NobleWith100HeavyCavalry, RandomNoble
    };
}
