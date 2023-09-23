using Application.Interfaces;
using Application.Models.AntennaTranslator;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/translatorTypes")]
[Authorize]
public class TranslatorTypesController : Controller
{
    private readonly IServiceWrapper _service;

    public TranslatorTypesController(IServiceWrapper service)
    {
        _service = service;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var baseResponse = _service.TranslatorTypeService.GetAll();
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var baseResponse = await _service.TranslatorTypeService.GetByOid(id);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]TranslatorTypeDto model)
    {
        var baseResponse = await _service.TranslatorTypeService.CreateAsync(model, User.Identity.Name);
        
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }
    
    [HttpPut]
    public async Task<IActionResult> Put(TranslatorTypeDto model)
    {
        var baseResponse = await _service.TranslatorTypeService.Update(model, User.Identity.Name);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }

    [HttpGet("index")]
    public async Task<IActionResult> Get([FromQuery]DataSourceLoadOptionsBase loadOptions)
    {
        var loadResult = await _service.TranslatorTypeService.GetLoadResult(loadOptions);
        return Ok(loadResult);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var baseResponse = await _service.TranslatorTypeService.Delete(id);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
}