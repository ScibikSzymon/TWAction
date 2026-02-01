namespace ActionGenerator.Domain.Common.ValueObjects;

public sealed record Coordinates
{
    public int X { get; init; }
    public int Y { get; init; }

    public double CalculateDistance(Coordinates target)
    {
        return Math.Sqrt(Math.Pow(X - target.X, 2) + Math.Pow(Y - target.Y, 2));
    }
}
