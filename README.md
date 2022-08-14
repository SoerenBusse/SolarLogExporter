# SolarLogExporter
This exporter collects data from local SolarLog devices and pushes it to an influx database.

Tested with SolarLog 500 and 3 SMA inverters.

**It requires at least firmware 3.x, because older firmwares don't provide a JSON HTTP endpoint**

## Usage
In case authentication is enabled on the SolarLog, `Offene JSON-Schnittstelle` has to be enabled manually in `Konfiguration / System / Zugangskontrolle`.

### Run the exporter
#### Using docker
This repository provides Docker images using the GitHub Container Registry to get things running easily: [Images](https://github.com/SoerenBusse/SolarLogExporter/pkgs/container/solarlog-exporter)

Sample docker-compose.yml:

```
solarlog-exporter:
  image: ghcr.io/soerenbusse/solarlog-exporter:1
  container_name: solarlog-exporter
  environment:
    - "SOLARLOG__URL=http://solarlog-server"
    - "SOLARLOG__LOCATION=House" # Will be added as a tag in the Influx datapoints
    - "INFLUX__URL=https://influxdb.domain.tld"
    - "INFLUX__BUCKET=Solar"
    - "INFLUX__ORGANISATION=MyOrganization"
    - "INFLUX__TOKEN=<TOKEN>
    # Default value 15 seconds. More doesn't make sense, 
    # because SolarLog doesn't update the date more often.
    - "POLLING__INTERVALSECONDS=15" 
  restart: always
```

See `src/SolarLogExporter/appsettings.sample.json` for all available configuration options.

#### Using .NET

You can also install the [.NET SDK](https://dotnet.microsoft.com/) on your system and run the exporter this way:
- Pull this repository and navigate into `src/SolarLogExporter`
- Create an `appsettings.json` (see `appsettings.sample.json`) with your configuration options
- Execute `dotnet run -c Release`

If needed, you can enable extended logging by setting the default log level in `appsettings.json` to `Debug`.

## SolarLog API
Since firmware version 3.x SolarLog provides a JSON HTTP-API-Endpoint.
However only the endpoint to fetch the total production of all inverters is documented in the manual.
All other endpoints used by the exporter are reversed-engineered and documented here.

**Base-URL**: `https://solarlog-server/getjq`

### Fetch current production of all inverters
```
curl https://solarlog-server/getjp --data '{"801":{"170":null}}'
{
    "801":{
        "170":{
            "101":2811, // Total AC power of all inverters
            "102":2914, // Total DC power of all inverters
        }
    }
}
```

### Fetch names and power of available inverters
```
curl https://solarlog-server/getjp --data '{"141":{"32000":{"118":null,"119":null}}'
{
    "141":{
        "0":{
            "118":17100, // Max power of inverter
            "119":"WR 3" // Name of inverter
        },
        "1":{
            "118":17100,
            "119":"WR 2"
        },
        "2":{
            "118":17100,
            "119":"WR 1"
       }
   }
}
```

### Fetch current production per inverter
```
curl https://solarlog-server/getjp --data '{"608":null,"782":null}}'
{
    // Status per inverter
    "608":{
        "0":"MPP",
        "1":"MPP",
        "2":"MPP",
        ...
        "99":"OFFLINE"
    },
    
    // Current production per inverter
    "782":{
        "0":"955",
        "1":"766",
        "2":"834",
        ...
        "99":"0"
    }
}
```
