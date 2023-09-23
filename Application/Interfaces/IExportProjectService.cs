using Application.DataObjects;

namespace Application.Interfaces;

public interface IExportProjectService
{
    Task<BaseResponse<byte[]>> GetProjectFile(string id);
}