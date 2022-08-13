using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolarLogExporter.Constants;
using SolarLogExporter.Exceptions;
using SolarLogExporter.Models;
using SolarLogExporter.Options;

namespace SolarLogExporter.Services;

public class SolarLogService
{
    public volatile SolarLogMeasurement? SolarLogMeasurement;

    private readonly HttpClient _httpClient;

    private readonly ILogger<SolarLogService> _logger;
    private readonly IOptions<SolarLogOptions> _solarLogOptions;
    private readonly Uri _baseUri;

    private Dictionary<int, InverterSpecification>? _inverterSpecifications;

    public SolarLogService(IHttpClientFactory httpClientFactory, ILogger<SolarLogService> logger,
        IOptions<SolarLogOptions> solarLogOptions)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(5);

        _logger = logger;
        _solarLogOptions = solarLogOptions;

        _baseUri = new Uri($"{solarLogOptions.Value.Url}/getjp");
    }

    public async Task ReadCurrentProduction()
    {
        try
        {
            var totalProductionResponse =
                await FetchDataFromServer(SolarLogActions.TotalProduction);

            // Get total production. It's encapsulated in multiple nested (unnecessary) keys
            var totalProduction = CheckKey(JObject.Parse(totalProductionResponse), "801", "170");

            // Retrieve AC and DC power based on the numeric keys
            var acPower = CheckKey(totalProduction, "101").Value<int>();
            var dcPower = CheckKey(totalProduction, "102").Value<int>();

            // Only fetch names if we don't have it already. It's very unlikely that somebody is deleting an inverter :)
            if (_inverterSpecifications == null)
            {
                Dictionary<int, InverterSpecification> inverterSpecifications = new();

                var inverterNamesResponse = await FetchDataFromServer(SolarLogActions.InverterNames);

                // The api design is very bad. We have to iterate over the keys of an object to find our inverter values
                var inverterNames = CheckKey(JObject.Parse(inverterNamesResponse), "141").ToObject<JObject>();

                // Check if 141 contains an object. Theres no documentation what 141 actually means, but it's there
                if (inverterNames == null)
                {
                    _logger.LogError("Cannot parse inverter names, because 141 doesn't contain an object");
                    return;
                }

                // Iterate over inverters as key value pair to get index and content
                foreach (var inverter in inverterNames)
                {
                    if (!int.TryParse(inverter.Key, out var index))
                    {
                        _logger.LogError("Cannot convert index of inverter to int");
                        return;
                    }

                    if (inverter.Value == null)
                    {
                        _logger.LogError($"Cannot find converter at index {index}");
                        return;
                    }

                    inverterSpecifications.Add(index, new InverterSpecification
                    {
                        MaxAcPower = CheckKey(inverter.Value, "118").Value<int>(),
                        Name = CheckKey(inverter.Value, "119").Value<string>()
                    });
                }

                // Update if everything was successful
                _inverterSpecifications = inverterSpecifications;
            }

            var inverterProductionResponse = await FetchDataFromServer(SolarLogActions.InverterProduction);

            var inverterProduction = CheckKey(JObject.Parse(inverterProductionResponse), "782").ToObject<JObject>();

            // Null if 782 is no object
            if (inverterProduction == null)
            {
                _logger.LogError("Cannot get inverter production, because 782 doesn't contain an json object");
                return;
            }

            List<Inverter> inverters = new List<Inverter>();

            // Iterate over known inverters
            foreach (KeyValuePair<int, InverterSpecification> inverterSpecification in _inverterSpecifications)
            {
                if (!inverterProduction.ContainsKey(inverterSpecification.Key.ToString()))
                {
                    _logger.LogError(
                        $"Cannot find inverter with index {inverterSpecification.Key} in production reply");
                    return;
                }

                inverters.Add(new Inverter
                {
                    AcPower = CheckKey(inverterProduction, inverterSpecification.Key.ToString()).Value<int>(),
                    Name = inverterSpecification.Value.Name,
                    MaxAcPower = inverterSpecification.Value.MaxAcPower
                });
            }

            // Create SolarSystem object with all fetched values
            var solarLogMeasurement = new SolarLogMeasurement
            {
                TotalAcPower = acPower,
                TotalDcPower = dcPower,
                Inverters = inverters,
                Location = _solarLogOptions.Value.Location
            };

            // Assign to global variable. Because assignment is atomic there are no inconsistency problems
            SolarLogMeasurement = solarLogMeasurement;
        }
        catch (SolarLogHttpException e)
        {
            throw new ReadProductionException(e.Message);
        }
        catch (JsonReaderException e)
        {
            throw new ReadProductionException($"Cannot parse json response: {e.Message}");
        }
        catch (HttpRequestException e)
        {
            // TODO: Or shouldn't we map it to ReadProductionException and throw it instead so the program terminates?
            // Should the service restart if theres a temporary loss in communication?
            throw new ReadProductionException($"Cannot reach the SolarLog server: {e.Message}");
        }
        catch (TaskCanceledException e) when (e.InnerException is TimeoutException)
        {
            throw new ReadProductionException($"Cannot reach the SolarLog server: {e.Message}");
        }
    }

    private static JToken CheckKey(JToken token, params string[] keys)
    {
        JToken? resource = token;

        if (keys.Length == 0)
        {
            throw new ArgumentException("Keys cannot be empty");
        }

        foreach (var key in keys)
        {
            resource = resource[key];

            if (resource == null)
            {
                throw new SolarLogHttpException($"JSON response missing key {key}");
            }
        }

        return resource;
    }

    private async Task<string> FetchDataFromServer(string action)
    {
        using var response =
            await _httpClient.PostAsync(_baseUri, new StringContent(action, Encoding.UTF8));

        if (!response.IsSuccessStatusCode)
        {
            throw new SolarLogHttpException($"Cannot fetch action {action}. Status Code: {response.StatusCode}");
        }

        var result = await response.Content.ReadAsStringAsync();

        if (result == null)
        {
            throw new SolarLogHttpException($"Cannot get content for action {action}");
        }

        return result;
    }
}