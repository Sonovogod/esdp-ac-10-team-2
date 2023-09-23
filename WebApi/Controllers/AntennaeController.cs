using Application.Interfaces;
using Application.Models.Antennae;
using AutoMapper;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/antennae")]
[Authorize]
public class AntennasController : Controller
{
    private readonly IServiceWrapper _service;
    private readonly IMapper _mapper;
   
    public AntennasController(IServiceWrapper service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var baseResponse = _service.AntennaService.GetAll();
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    
    [HttpGet("{oid}")]
    public async Task<IActionResult> Get(string oid)
    {
        var baseResponse = await _service.AntennaService.GetByOid(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateAntennaDto model)
    {
        AntennaDto antennaDto = _mapper.Map<AntennaDto>(model);
        var baseResponse = await _service.AntennaService.CreateAsync(antennaDto, User.Identity.Name);
        
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UpdateAntennaDto model)
    {
        var baseResponse = await _service.AntennaService.Update(model, User.Identity.Name);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }
        
    [HttpDelete("{oid}")]
    public async Task<IActionResult> Delete(string oid)
    {
        var baseResponse = await _service.AntennaService.Delete(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    [HttpGet("index")]
    public async Task<IActionResult> Get([FromQuery]DataSourceLoadOptionsBase loadOptions)
    {
        var loadResult = await _service.AntennaService.GetLoadResult(loadOptions);
        return Ok(loadResult);
    }
}