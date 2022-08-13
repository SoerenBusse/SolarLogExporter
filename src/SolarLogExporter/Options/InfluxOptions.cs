using System.ComponentModel.DataAnnotations;

namespace SolarLogExporter.Options;

public class InfluxOptions
{
    public const string Key = "Influx";

    [Required] public string? Url { get; init; }
    [Required] public string? Bucket { get; init; }
    [Required] public string? Organisation { get; init; }
    [Required] public string? Token { get; init; }
}