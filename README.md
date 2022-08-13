# SolarLogExporter

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
