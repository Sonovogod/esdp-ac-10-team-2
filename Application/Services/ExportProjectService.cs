using Application.DataObjects;
using Application.Interfaces;
using Application.Interfaces.RepositoryContract.Common;

namespace Application.Services;

public class ExportProjectService : IExportProjectService
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public ExportProjectService(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<BaseResponse<byte[]>> GetProjectFile(string id)
    {
        var project = await _repositoryWrapper.ProjectRepository.GetByCondition(x => x.Id.ToString() == id);
        string mainDir = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(mainDir,"TemporaryFiles",$"Project{project.Id}.docx");
        if (!File.Exists(filePath))
            return new BaseResponse<byte[]>(
                Result: null,
                Messages: new List<string>() {"Файл проекта для экспорта не найден"},
                Success: false);
        
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        
        return new BaseResponse<byte[]>(
            Result: fileBytes,
            Messages: new List<string>() {"Файл проекта для экспорта получен"},
            Success: true);
    }
}