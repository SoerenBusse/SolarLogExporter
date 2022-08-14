using Microsoft.Extensions.Options;
using SolarLogExporter.Exceptions;
using SolarLogExporter.Options;

namespace SolarLogExporter.Services;

public class DevicePollingService : IHostedService, IDisposable
{
    private readonly ILogger<DevicePollingService> _logger;
    private readonly IOptions<PollingOptions> _pollingOptions;
    private readonly SolarLogService _solarLogService;
    private readonly InfluxService _influxService;

    private Timer? _timer;

    public DevicePollingService(ILogger<DevicePollingService> logger, IOptions<PollingOptions> pollingOptions,
        SolarLogService solarLogService, InfluxService influxService)
    {
        _logger = logger;
        _pollingOptions = pollingOptions;
        _solarLogService = solarLogService;
        _influxService = influxService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting SolarLog polling...");

        await Update();

        TimeSpan interval = TimeSpan.FromSeconds(_pollingOptions.Value.IntervalSeconds);
        _timer = new Timer(OnTimerTick, null, interval, interval);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping SolarLog polling...");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void OnTimerTick(object? state)
    {
        await Update();
    }

    private async Task Update()
    {
        _logger.LogDebug("Polling SolarLog...");

        // Read the current production of the solar system
        try
        {
            await _solarLogService.ReadCurrentProduction();

            // Add result to influx db if we got some data
            if (_solarLogService.SolarLogMeasurement != null)
            {
                await _influxService.PushSolarLogMeasurement(_solarLogService.SolarLogMeasurement);
            }
        }
        catch (ReadProductionException e)
        {
            _logger.LogError(e.Message);
        }
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
    }
}