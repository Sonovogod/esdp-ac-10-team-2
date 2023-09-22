using Application.DataObjects;
using Application.Extensions;
using Application.Interfaces;
using Application.Models.RadiationZone;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace Application.Services;

public class RadiationZoneExelFileService : IRadiationZoneExelFileService
{
    private readonly IRadiationZoneService _radiationZoneService;

    public RadiationZoneExelFileService(IRadiationZoneService radiationZoneService)
    {
        _radiationZoneService = radiationZoneService;
    }

    public async Task<BaseResponse<string>> CreateAsync(string translatorId, IFormFile? verticalFile,
        IFormFile horizontalFile, string creator)
    {
        if (!Guid.TryParse(translatorId, out var id))
        {
            return new BaseResponse<string>(
                Result: null,
                Messages: new List<string>(){"Ошибка при получении ID транслятора"},
                Success: false);
        }
        
        var convertHorizontalFile = await ConvertToRadiationZoneDto(id, horizontalFile, DirectionType.Horizontal);
        var convertVerticalFile = await ConvertToRadiationZoneDto(id, verticalFile, DirectionType.Vertical);

        if (!convertVerticalFile.Success)
        {
            return new BaseResponse<string>(
                Result: null,
                Messages: convertVerticalFile.Messages,
                Success: false);
        }
        if (!convertHorizontalFile.Success)
        {
            return new BaseResponse<string>(
                Result: null,
                Messages: convertHorizontalFile.Messages,
                Success: false);
        }

        var createVerticalResponse = await CreateInDb(convertVerticalFile.Result, creator);
        var createHorizontalResponse = await CreateInDb(convertHorizontalFile.Result, creator);
        
        if (!createVerticalResponse.Success)
        {
            return new BaseResponse<string>(
                Result: null,
                Messages: createVerticalResponse.Messages,
                Success: false);
        }
        if (!createHorizontalResponse.Success)
        {
            return new BaseResponse<string>(
                Result: null,
                Messages: createHorizontalResponse.Messages,
                Success: false);
        }

        return new BaseResponse<string>(
            Result: null,
            Messages: new List<string>() {"Передатчики успешно сохранены"},
            Success: true);
    }

    public async Task<BaseResponse<byte[]>> GetTemplate()
    {
        string mainDir = AppDomain.CurrentDomain.BaseDirectory;
        string templatePath = Path.Combine(mainDir, "Template", "template.xlsx");
        if (!File.Exists(templatePath))
            return new BaseResponse<byte[]>(
                Result: null,
                Messages: new List<string>() {"Шаблон не найден"},
                Success: false);
        var fileBytes = await File.ReadAllBytesAsync(templatePath);
       
        return new BaseResponse<byte[]>(
            Result: fileBytes,
            Messages: new List<string>() {"Шаблон получен"},
            Success: true);
    }

    private async Task<BaseResponse<string>> CreateInDb (List<RadiationZoneDto> models, string creator)
    {
        foreach (var model in models)
        {
            var response = await _radiationZoneService.CreateAsync(model, creator);
            if (!response.Success)
            {
                return new BaseResponse<string>(
                    Result: null,
                    Messages: response.Messages,
                    Success: false);
            }
        }
        
        return new BaseResponse<string>(
            Result: null,
            Messages: new List<string>(){"Передатчики успешно сохранены"},
            Success: true);
    }

    private async Task<BaseResponse<List<RadiationZoneDto>>> ConvertToRadiationZoneDto(Guid translatorId, IFormFile? file,
        DirectionType directionType)
    {
        BaseResponse<string> fileResponse = await SaveFile(file);

        if (!fileResponse.Success)
        {
            return new BaseResponse<List<RadiationZoneDto>>(
                Result: null,
                Messages: fileResponse.Messages,
                Success: false);
        }

        BaseResponse<List<RadiationZoneDto>> models =
            CreateRadiationZoneModels(fileResponse.Result, directionType, translatorId);

        if (!models.Success)
        {
            return new BaseResponse<List<RadiationZoneDto>>(
                Result: null,
                Messages: models.Messages,
                Success: false);
        }

        return models;
    }

    private void Delete(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private async Task<BaseResponse<string>> SaveFile(IFormFile? uploadedFile)
    {
        string[] allowedExtension = { ".csv", ".xlsx", ".xlsm" };
        if (uploadedFile is not null)
        {
            string mainDir = Environment.CurrentDirectory;
            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();
            var folderPath = (mainDir + @"\TemporaryFiles");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        
            var fileName = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{fileExtension}";
            string filePath = Path.Combine(folderPath, fileName);
        
            if (allowedExtension.Any(extension => extension == fileExtension))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create ))
                {
                    if (fileStream.Length < 4097152)
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }
                    else
                    {
                        return new BaseResponse<string>(
                            Result: null,
                            Messages: new List<string> { "Размер файла больше допустимого (4Mb)" },
                            Success: false);
                    }
                }    
                return new BaseResponse<string>(
                    Result: filePath,
                    Messages: new List<string> { "Файл успешно сохранен" },
                    Success: true);
            }
            return new BaseResponse<string>(
                Result: null,
                Messages: new List<string> { "Расширение файла не допустимо" },
                Success: false);
        }
        return new BaseResponse<string>(
            Result: null,
            Messages: new List<string> { "Ошибка получения файла на сервере" },
            Success: false);
    }

    private BaseResponse<List<RadiationZoneDto>> CreateRadiationZoneModels(string filePath, DirectionType type,
        Guid translatorSpecsId)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<RadiationZoneDto> list = new List<RadiationZoneDto>();
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))    
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                    {
                        var degreeCellValue = worksheet.Cells[row, 1].Value;
                        var valueCellValue = worksheet.Cells[row, 2].Value;
                        if (int.TryParse(degreeCellValue?.ToString(), out int degree) &&
                            decimal.TryParse(valueCellValue?.ToString(), out decimal value))
                        {
                            RadiationZoneDto radiationZone = new RadiationZoneDto()
                            {
                                Degree = degree,
                                Value = value,
                                DirectionType = type.GetDescription(),
                                TranslatorSpecsId = translatorSpecsId
                            };
                            list.Add(radiationZone);
                        }
                    }

                    if (list.Count != 361)
                    {
                        return new BaseResponse<List<RadiationZoneDto>>(
                            Result: list,
                            Messages: new List<string> { "Файл не корректный" },
                            Success: false);
                    }
                }
            }

            return new BaseResponse<List<RadiationZoneDto>>(
                Result: list,
                Messages: new List<string> { "Файл успешно считан" },
                Success: true);
        }
        catch (Exception e)
        {
            return new BaseResponse<List<RadiationZoneDto>>(
                Result: null,
                Messages: new List<string> { "Ошибка при создании моделей" + e.Message },
                Success: false);
        }
        finally
        {
            Delete(filePath);
        }
    }
}