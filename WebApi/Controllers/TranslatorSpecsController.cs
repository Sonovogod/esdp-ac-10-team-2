using Application.Interfaces;
using Application.Models.TranslatorSpecs;
using AutoMapper;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/translators")]
[Authorize]
public class TranslatorSpecsController : Controller
{
    private readonly IServiceWrapper _service;
    private readonly IMapper _mapper;
   
    public TranslatorSpecsController(
        IServiceWrapper service, 
        IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var baseResponse = _service.TranslatorSpecsService.GetAll();
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    [HttpGet("getAll/{id}")]
    public IActionResult GetAll(string id)
    {
        var baseResponse = _service.TranslatorSpecsService.GetAllByProjectId(id);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    [HttpGet("{oid}")]
    public async Task<IActionResult> Get(string oid)
    {
        var baseResponse = await _service.TranslatorSpecsService.GetByOid(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm]CreateTranslatorSpecsDto translatorModel)
    {
        TranslatorSpecsDto translatorSpecsDto = _mapper.Map<TranslatorSpecsDto>(translatorModel);
        var baseResponse = await _service.TranslatorSpecsService
            .CreateAsync(translatorSpecsDto, User.Identity.Name);
        if (baseResponse.Success)
        {
            var response = await _service.RadiationZoneExelFileService
                .CreateAsync(baseResponse.Result, translatorModel.Vertical, translatorModel.Horizontal, User.Identity.Name);
            if (response.Success)
            {
                return Ok(baseResponse);
            }

            await _service.TranslatorSpecsService.Delete(baseResponse.Result);
            return Ok(response);
        }
        return Ok(baseResponse);
    }

    [HttpPut]
    public async Task<IActionResult> Put(TranslatorSpecsDto model)
    {
        var baseResponse = await _service.TranslatorSpecsService.Update(model, User.Identity.Name);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }
        
    [HttpDelete("{oid}")]
    public async Task<IActionResult> Delete(string oid)
    {
        var baseResponse = await _service.TranslatorSpecsService.Delete(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
    
    [HttpGet("index/{id}")]
    public async Task<IActionResult> Get(string id, [FromQuery]DataSourceLoadOptionsBase loadOptions)
    {
        var loadResult = await _service.TranslatorSpecsService.GetLoadResult(id, loadOptions);
        return Ok(loadResult);
    }
    
    [HttpGet("getAllByAntennaId/{id}")]
    public IActionResult GetAllByAntennaId(string id)
    {
        var baseResponse = _service.TranslatorSpecsService.GetAllByAntennaId(id);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
}