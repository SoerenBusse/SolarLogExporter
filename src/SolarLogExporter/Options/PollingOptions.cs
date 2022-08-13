using System.ComponentModel.DataAnnotations;

namespace SolarLogExporter.Options;

public class PollingOptions
{
    public const string Key = "Polling";

    [Range(1, int.MaxValue)] public int IntervalSeconds { get; init; } = 5;
}