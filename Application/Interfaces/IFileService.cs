using Application.DataObjects;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IFileService
{
    public Task<BaseResponse<bool>> GetLoadXlsx();

    public Task<BaseResponse<bool>> ReadExcel(string filePath, TranslatorSpecs translatorSpecs, DirectionType type);
    public Task<BaseResponse<bool>> ProjectWord(string oid);

}