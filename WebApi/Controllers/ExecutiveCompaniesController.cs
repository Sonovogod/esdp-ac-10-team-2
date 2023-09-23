using Application.Interfaces;
using Application.Models.ExecutiveCompany;
using AutoMapper;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExecutiveCompaniesController : Controller
    {
        private readonly IServiceWrapper _service;
        private readonly IMapper _mapper;

        public ExecutiveCompaniesController(IMapper mapper, IServiceWrapper service)
        {
            _mapper = mapper;
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var baseResponse = _service.ExecutiveCompanyService.GetAll();
            if (baseResponse.Success)
                return Ok(baseResponse);
            return NotFound(baseResponse);
        }

        [HttpGet("{oid}")]
        public async Task<IActionResult> Get(string oid)
        {
            var baseResponse = await _service.ExecutiveCompanyService.GetByOid(oid);
            if (baseResponse.Success)
                return Ok(baseResponse);
            return NotFound(baseResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateExecutiveCompanyDto model)
        {
            ExecutiveCompanyDto companyDto = _mapper.Map<ExecutiveCompanyDto>(model);
            var baseResponse = await _service.ExecutiveCompanyService.CreateAsync(companyDto, User.Identity.Name);

            if (baseResponse.Success)
                return Ok(baseResponse);
            return BadRequest(baseResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Put(UpdateExecutiveCompanyDto model)
        {
            var baseResponse = await _service.ExecutiveCompanyService.UpdateExecutiveCompany(model, User.Identity.Name);
            if (baseResponse.Success)
                return Ok(baseResponse);
            return BadRequest(baseResponse);
        }

        [HttpDelete("{oid}")]
        public async Task<IActionResult> Delete(string oid)
        {
            var baseResponse = await _service.ExecutiveCompanyService.Delete(oid);
            if (baseResponse.Success)
                return Ok(baseResponse);
            return NotFound(baseResponse);
        }

        [HttpGet("index")]
        public async Task<IActionResult> Get([FromQuery] DataSourceLoadOptionsBase loadOptions)
        {
            var loadResult = await _service.ExecutiveCompanyService.GetLoadResult(loadOptions);
            return Ok(loadResult);
        }
    }
}