using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
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

        DateTime timestamp = DateTime.UtcNow;

        var pointData = new List<PointData>();

        pointData.Add(PointData
            .Measurement("solarlog_total")
            .Timestamp(timestamp, WritePrecision.S)
            .Tag("location", solarLogMeasurement.Location)
            .Field("ac_power", solarLogMeasurement.TotalAcPower)
            .Field("dc_power", solarLogMeasurement.TotalDcPower));

        var solarLogInverter = PointData
            .Measurement("solarlog_inverter")
            .Timestamp(timestamp, WritePrecision.S)
            .Tag("location", solarLogMeasurement.Location);

        // Create data points for each inverter
        pointData.AddRange(solarLogMeasurement.Inverters.Select(inverter => solarLogInverter
            .Tag("name", inverter.Name)
            .Field("ac_power", inverter.AcPower)
            .Field("max_ac_power", inverter.MaxAcPower)));

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