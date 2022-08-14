using System.Text.Json.Serialization;
using SolarLogExporter.Options;
using SolarLogExporter.Services;

namespace SolarLogExporter;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<SolarLogOptions>().Bind(Configuration.GetSection(SolarLogOptions.Key))
            .ValidateDataAnnotations();
        services.AddOptions<InfluxOptions>().Bind(Configuration.GetSection(InfluxOptions.Key))
            .ValidateDataAnnotations();
        services.AddOptions<PollingOptions>().Bind(Configuration.GetSection(PollingOptions.Key))
            .ValidateDataAnnotations();

        services.AddHttpClient();

        // API Endpoints
        services.AddControllers().AddXmlSerializerFormatters().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.WriteIndented = true;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddSingleton<InfluxService>();
        services.AddSingleton<SolarLogService>();

        // Add polling service
        services.AddHostedService<DevicePollingService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoint =>
        {
            // Info text for web requests
            endpoint.MapGet("/", context => context.Response.WriteAsync("SolarLog Exporter"));

            endpoint.MapControllers();
        });
    }
}