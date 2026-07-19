using ActionGenerator.MainAction;

namespace ActionGenerator.Application.Common.Services;

public interface INightTimeChecker
{
    bool IsNightTime(DateTimeOffset time);
}

internal sealed class NightTimeChecker : INightTimeChecker
{
    public bool IsNightTime(DateTimeOffset time) => NightTimeHelper.IsNightTime(time);
}
