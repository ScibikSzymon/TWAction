namespace ActionGenerator.MainAction;

public static class NightTimeHelper
{
    private const int NightStartHour = 23;
    private const int NightEndHour = 7;

    public static bool IsNightTime(DateTimeOffset time)
    {
        var hour = time.Hour;
        return hour >= NightStartHour || hour < NightEndHour;
    }
}
