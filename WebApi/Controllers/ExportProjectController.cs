using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/export-project")]
[ApiController]
[Authorize]
public class ExportProjectController : Controller
{
    private readonly IServiceWrapper _service;

    public ExportProjectController(IServiceWrapper service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<FileResult> Index(string id)
    {
        var file = await _service.FileService.ProjectWord(id);
        return File(file.Result, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }
}