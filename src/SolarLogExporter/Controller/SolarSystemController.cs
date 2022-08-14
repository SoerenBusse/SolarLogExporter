using Microsoft.AspNetCore.Mvc;
using SolarLogExporter.Models;
using SolarLogExporter.Services;

namespace SolarLogExporter.Controller;

[ApiController]
[Route("measurements")]
public class MeasurementsController : ControllerBase
{
    private readonly SolarLogService _solarLogService;

    public MeasurementsController(SolarLogService solarLogService)
    {
        _solarLogService = solarLogService;
    }

    [HttpGet]
    public ActionResult<SolarLogMeasurement> GetMeasurement() =>
        _solarLogService.SolarLogMeasurement ?? (ActionResult<SolarLogMeasurement>) NotFound();
}