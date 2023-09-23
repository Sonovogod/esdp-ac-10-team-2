using Application.Interfaces;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/radiationZones")]
[Authorize]
public class RadiationZoneController : Controller
{
    private readonly IServiceWrapper _service;

    public RadiationZoneController(
        IServiceWrapper service)
    {
        _service = service;
    }
    
    [HttpGet("index/translator/{id}")]
    public async Task<IActionResult> Get(string id, [FromQuery]DataSourceLoadOptionsBase loadOptions)
    {
        var baseResponse = await _service.RadiationZoneService.GetLoadResultById(id, loadOptions);
        return Ok(baseResponse);
    }

    [HttpGet("template")]
    public async Task<FileResult> Get()
    {
        var file = await _service.RadiationZoneExelFileService.GetTemplate();
        return File(file.Result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "template.xlsx");
    }
}