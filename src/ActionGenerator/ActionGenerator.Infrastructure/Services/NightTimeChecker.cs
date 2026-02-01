using ActionGenerator.Application.Common.Interfaces;

namespace ActionGenerator.Infrastructure.Services;

public sealed class NightTimeChecker : INightTimeChecker
{
    private readonly TimeSpan _nightStart = new(22, 0, 0);
    private readonly TimeSpan _nightEnd = new(8, 0, 0);

    public bool IsNightTime(DateTimeOffset time)
    {
        var timeOfDay = time.TimeOfDay;
        
        return timeOfDay >= _nightStart || timeOfDay < _nightEnd;
    }
}
