using Application.Interfaces;
using Application.Models.ContrAgents;
using AutoMapper;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContrAgentsController : Controller
{
    
    private readonly IServiceWrapper _service;
    private readonly IMapper _mapper;

    public ContrAgentsController(IMapper mapper, IServiceWrapper service)
    {
        _mapper = mapper;
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var baseResponse = _service.ContrAgentService.GetAll();
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    
    [HttpGet("{oid}")]
    public async Task<IActionResult> Get(string oid)
    {
        var baseResponse = await _service.ContrAgentService.GetByOid(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateContrAgentDto model)
    {
        ContrAgentDto contrAgentDto = _mapper.Map<ContrAgentDto>(model);
        var baseResponse = await _service.ContrAgentService.CreateAsync(contrAgentDto, User.Identity.Name);
        
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UpdateContrAgentDto model)
    {
        var baseResponse = await _service.ContrAgentService.UpdateContrAgent(model, User.Identity.Name);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }
        
    [HttpDelete("{oid}")]
    public async Task<IActionResult> Delete(string oid)
    {
        var baseResponse = await _service.ContrAgentService.Delete(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    [HttpGet("index")]
    public async Task<IActionResult> Get([FromQuery]DataSourceLoadOptionsBase loadOptions)
    {
        var loadResult = await _service.ContrAgentService.GetLoadResult(loadOptions);
        return Ok(loadResult);
    }
}