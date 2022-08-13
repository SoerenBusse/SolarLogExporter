using System.ComponentModel.DataAnnotations;

namespace SolarLogExporter.Options;

public class SolarLogOptions
{
    public const string Key = "SolarLog";

    [Required] public string? Url { get; init; }

    [Required] public string? Location { get; init; }
}