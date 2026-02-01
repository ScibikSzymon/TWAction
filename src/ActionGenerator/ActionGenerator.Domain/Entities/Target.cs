using ActionGenerator.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActionGenerator.Domain.Entities;

public class Target :Village
{
    public required DateTimeOffset MinArrivalTime { get; init; }
    public required DateTimeOffset MaxArrivalTime { get; init; }
    public required CommandType CommandType { get; init; }
}
