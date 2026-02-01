using ActionGenerator.Application.Common.Interfaces;

namespace ActionGenerator.Infrastructure.Services;

public sealed class DistanceCalculator : IDistanceCalculator
{
    public double CalculateDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }
}
