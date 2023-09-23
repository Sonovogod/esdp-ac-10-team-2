using Application.Interfaces;
using Application.Models.Projects.ProjectImages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/project-images")]
[Authorize]
public class ProjectImagesControllers : Controller
{
    private readonly IServiceWrapper _service;

    public ProjectImagesControllers(IServiceWrapper service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        var baseResponse = _service.ProjectImageService.GetAllById(id);
        return Ok(baseResponse);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromForm]ProjectImageDto model, [FromForm]IFormFile uploadedFile)
    {
        var saveFileResponse = await _service.ProjectImageService.ConvertImage(model, uploadedFile);
        if (saveFileResponse.Success)
        {
            var baseResponse = await _service.ProjectImageService.CreateAsync(saveFileResponse.Result, User.Identity.Name);
            
            return Ok(baseResponse);
        }
        
        return Ok(saveFileResponse);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var baseResponse = await _service.ProjectImageService.Delete(id);
        if (baseResponse.Success)
            return Ok(baseResponse);
        return NotFound(baseResponse);
    }
}