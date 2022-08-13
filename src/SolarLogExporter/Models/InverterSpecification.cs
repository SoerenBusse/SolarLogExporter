namespace SolarLogExporter.Models;

public record InverterSpecification
{
    public string? Name { get; init; }
    public int MaxAcPower { get; init; }
}