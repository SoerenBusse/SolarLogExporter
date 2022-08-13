namespace SolarLogExporter.Models;

public record SolarLogMeasurement
{
    public int TotalAcPower { get; init; }
    public int TotalDcPower { get; init; }

    public string? Location { get; init; }
    public IEnumerable<Inverter> Inverters { get; init; } = new List<Inverter>();
}

public record Inverter
{
    public string? Name { get; init; }
    public int AcPower { get; init; }
    public int MaxAcPower { get; init; }
}