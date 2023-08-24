using Application.Interfaces;
using Application.Models.Antennae;
using Application.Models.Antennas;
using Application.Models.Projects.ProjectAntennas;
using AutoMapper;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/projects-antenna")]
public class ProjectAntennaController : Controller
{
    private readonly IServiceWrapper _service;
    private readonly IMapper _mapper;

    public ProjectAntennaController(IServiceWrapper service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var baseResponse = _service.ProjectAntennaService.GetAll();
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    [HttpGet("index")]
    public async Task<IActionResult> Get([FromQuery]DataSourceLoadOptionsBase loadOptions)
    {
        var loadResult = await _service.ProjectAntennaService.GetLoadResult(loadOptions);
        return Ok(loadResult);
    }

    [HttpGet("getAllFromThisProject")]
    public IActionResult GetAll(string id)
    {
        var baseResponse = _service.ProjectAntennaService.GetAllByProjectId(id);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    [HttpGet("{oid}")]
    public async Task<IActionResult> Get(string oid)
    {
        var baseResponse = await _service.ProjectAntennaService.GetByOid(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]CreateProjectAntennaDto model)
    {
        ProjectAntennaDto projectAntennaDto = _mapper.Map<ProjectAntennaDto>(model);
        var baseResponse = await _service.ProjectAntennaService.CreateAsync(projectAntennaDto, User.Identity.Name);
        
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UpdateProjectAntennaDto model)
    {
        var baseResponse = await _service.ProjectAntennaService.Update(model);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }

    [HttpDelete("{oid}")]
    public async Task<IActionResult> Delete(string oid)
    {
        var baseResponse = await _service.ProjectAntennaService.Delete(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
}