using Application.Interfaces;
using Application.Models.Roles;
using AutoMapper;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : Controller
{
    private readonly IServiceWrapper _service;
    private readonly IMapper _mapper;

    public RolesController(IServiceWrapper service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var baseResponse = _service.RoleService.GetAll();
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpGet("{oid}")]
    public async Task<IActionResult> Get(string oid)
    {
        var baseResponse = await _service.RoleService.GetByOid(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateRoleDto model)
    {
        RoleDto roleDto = _mapper.Map<RoleDto>(model);
        var baseResponse = await _service.RoleService.CreateAsync(roleDto, User.Identity.Name);

        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UpdateRoleDto model)
    {
        var baseResponse = await _service.RoleService.UpdateRole(model, User.Identity.Name);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return BadRequest(baseResponse);
    }

    [HttpDelete("{oid}")]
    public async Task<IActionResult> Delete(string oid)
    {
        var baseResponse = await _service.RoleService.Delete(oid);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }

    [HttpGet("index")]
    public async Task<IActionResult> Get([FromQuery] DataSourceLoadOptionsBase loadOptions)
    {
        var loadResult = await _service.RoleService.GetLoadResult(loadOptions);
        return Ok(loadResult);
    }
}