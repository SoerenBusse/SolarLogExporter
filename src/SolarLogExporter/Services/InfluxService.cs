using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using SolarLogExporter.Models;
using SolarLogExporter.Options;

namespace SolarLogExporter.Services;

public class InfluxService : IDisposable
{
    private const string Measurement = "SolarLog";

    private readonly ILogger<InfluxService> _logger;
    private readonly IOptions<InfluxOptions> _influxOptions;

    private readonly InfluxDBClient _influxDbClient;
    private readonly WriteApiAsync _writeApiAsync;

    public InfluxService(ILogger<InfluxService> logger, IOptions<InfluxOptions> influxOptions)
    {
        _logger = logger;
        _influxOptions = influxOptions;

        _influxDbClient = InfluxDBClientFactory.Create(influxOptions.Value.Url, influxOptions.Value.Token);
        _writeApiAsync = _influxDbClient.GetWriteApiAsync();
    }

    public async Task PushSolarLogMeasurement(SolarLogMeasurement solarLogMeasurement)
    {
        if (solarLogMeasurement.Location == null)
        {
            throw new ArgumentException("Location of SolarLogMeasurement cannot be null");
        }

        var location = solarLogMeasurement.Location;

        // Create data points
        var pointData = new List<PointData>();

        pointData.Add(BasePoint(location).Field("totalAcPower", solarLogMeasurement.TotalAcPower));
        pointData.Add(BasePoint(location).Field("totalDcPower", solarLogMeasurement.TotalDcPower));

        // Create data points for each inverter
        pointData.AddRange(solarLogMeasurement.Inverters.Select(inverter => BasePoint(location)
            .Tag("name", inverter.Name)
            .Tag("maxAcPower", inverter.MaxAcPower.ToString())
            .Field("inverterAcPower", inverter.AcPower)));

        await _writeApiAsync.WritePointsAsync(pointData, _influxOptions.Value.Bucket,
            _influxOptions.Value.Organisation);
    }

    private PointData BasePoint(string location)
    {
        return PointData.Measurement(Measurement).Tag("location", location);
    }

    public void Dispose()
    {
        _influxDbClient.Dispose();
    }
}