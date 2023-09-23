using System.Drawing;
using Application.DataObjects;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.RepositoryContract.Common;
using Application.Models.EnergyResult;
using DevExpress.Office.Services;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using Domain.Entities;
using Domain.Enums;
using DevExpress.Spreadsheet;
using DevExpress.Spreadsheet.Charts;
using DevExpress.XtraSpreadsheet.Services;
using ChartType = DevExpress.XtraRichEdit.API.Native.ChartType;
using DocumentFormat = DevExpress.XtraRichEdit.DocumentFormat;
using SearchOptions = DevExpress.XtraRichEdit.API.Native.SearchOptions;
using Shape = DevExpress.XtraRichEdit.API.Native.Shape;
using Table = DevExpress.XtraRichEdit.API.Native.Table;



namespace Application.Services;

public class FileService : IFileService
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IEnergyFlowService _energyFlowService;
    private readonly IBiohazardRadiusService _biohazardRadiusService;
    private readonly ITotalFluxDensityService _totalFluxDensityService;


    public FileService(IRepositoryWrapper repositoryWrapper, IEnergyFlowService energyFlowService, 
        IBiohazardRadiusService biohazardRadiusService,ITotalFluxDensityService totalFluxDensityService)
    {
        _repositoryWrapper = repositoryWrapper;
        _energyFlowService = energyFlowService;
        _biohazardRadiusService = biohazardRadiusService;
        _totalFluxDensityService = totalFluxDensityService;
    }

    public async Task<BaseResponse<byte[]>> ProjectWord(string oid)
    {
        try
        {
            string mainDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePathTemp = Path.Combine(mainDir,"Template","ProjectTemp.docx");
            OfficeCharts.Instance.ActivateCrossPlatformCharts();
            var project = await  _repositoryWrapper.ProjectRepository.GetByCondition(x =>
                x.Id.ToString() == oid);
            string filePathExport = Path.Combine(mainDir,"TemporaryFiles",$"Project{project.Id}.docx");
            await _biohazardRadiusService.Create(project.Id.ToString());
            await _energyFlowService.CreateAsync(project.Id.ToString(), project.CreatedBy);
            var contrAgent = project.ContrAgent;
            var year = (project.SanPinDock?.DateOfIssue.Year) ?? DateTime.Now.Year;
            var executor = project.Executor;
            var executiveCompany = project.ExecutiveCompany;
            using (var wordProcessor = new RichEditDocumentServer()) 
            { 
                wordProcessor.LoadDocument(filePathTemp);
                Document document = wordProcessor.Document;
                document.ReplaceAll("[ContrAgent]", $"{contrAgent.CompanyName}", SearchOptions.WholeWord);
                document.ReplaceAll("[ExecutiveCompanyName]", $"{executiveCompany.CompanyName}", SearchOptions.WholeWord);
                document.ReplaceAll("[ExecutiveCompanyBIN]", $"{executiveCompany.BIN}", SearchOptions.WholeWord);
                document.ReplaceAll("[ProjectAddress]", $"{project.Address}", SearchOptions.WholeWord);
                document.ReplaceAll("[ExecutiveCompanyAddress]", $"{executiveCompany.Address}", SearchOptions.WholeWord);
                document.ReplaceAll("[ExecutorFIO]", $"{executor.Surname} {executor.Name} {executor.Patronymic}", SearchOptions.WholeWord);
                document.ReplaceAll("[ExecutorEmail]", $"{executor.Login}", SearchOptions.WholeWord);
                document.ReplaceAll("[ExecutorNumber]", $"+7{executor.PhoneNumber}", SearchOptions.WholeWord);
                document.ReplaceAll("[License]", $"{executiveCompany.LicenseNumber} от {executiveCompany.LicenseDateOfIssue} г.", SearchOptions.WholeWord);
                document.ReplaceAll("[ProjectNumber]", $"{project.ProjectNumber}", SearchOptions.WholeWord);
                document.ReplaceAll("[ContrAgentPhone]", $"{contrAgent.PhoneNumber}", SearchOptions.WholeWord);
                document.ReplaceAll("[ContrAgentBIN]", $"{contrAgent.BIN}", SearchOptions.WholeWord);
                document.ReplaceAll("[ContrAgentFIO]", 
                    $"{contrAgent.DirectorSurname} {contrAgent.DirectorName} {contrAgent.DirectorPatronymic}", SearchOptions.WholeWord);
                document.ReplaceAll("[ContrAgentAddress]", $"{contrAgent.Address}", SearchOptions.WholeWord);
                document.ReplaceAll("[DateYear]", $"{DateTime.Now.Year}", SearchOptions.WholeWord);
                document.ReplaceAll("[YearOfInitial]", $"{year}", SearchOptions.WholeWord);
                document.ReplaceAll("[PurposeRTO]", $"{project.PurposeRto}", SearchOptions.WholeWord);
                document.ReplaceAll("[PlaceOfInstall]", $"{project.PlaceOfInstall}", SearchOptions.WholeWord);
                document.ReplaceAll("[MaxHeightAdjoinBuild]", $"{project.MaxHeightAdjoinBuild}", SearchOptions.WholeWord);
                document.ReplaceAll("[PurposeBuild]", $"{project.PurposeBuild}", SearchOptions.WholeWord);
                document.ReplaceAll("[TypeORoof]", $"{project.TypeORoof}", SearchOptions.WholeWord);
                document.ReplaceAll("[HasTechnicalLevel]", project.HasTechnicalLevel != null ? "да" : "нет", SearchOptions.WholeWord);
                document.ReplaceAll("[TypeOfTopCover]", $"{project.TypeOfTopCover}", SearchOptions.WholeWord);
                document.ReplaceAll("[HasOtherRTO]", project.HasOtherRto != null ? "да" : "нет", SearchOptions.WholeWord);
                document.ReplaceAll("[PlaceOfCommunicationCloset]", $"{project.PlaceOfCommunicationCloset}", SearchOptions.WholeWord);
                document.ReplaceAll("[ExecutiveCompanyDirectory]", $"{executiveCompany.DirectorSurname}  " +
                                                                   $"{executiveCompany.DirectorName} {executiveCompany.DirectorPatronymic}", SearchOptions.WholeWord);
                var projectAntennae = _repositoryWrapper.ProjectAntennaRepository
                    .GetAllByCondition(x=> x.ProjectId == project.Id).ToList();
                var countTranslators = 0;
                var countAntenna = 0;
                for (int l = 0; l < projectAntennae.Count; l++)
                {
                    countAntenna++;
                    var gain = "";
                    var power = "";
                    var frequency = "";
                    var type = "";
                    var tilt = "";
                    var powerList = new List<string>();
                    var gainList = new List<string>();
                    var frequencyList = new List<string>();
                    var typeList = new List<string>();
                    var tiltList = new List<string>();
                    var antennaTranslatorId = Guid.Empty;
                    var antennaTranslators = _repositoryWrapper.AntennaTranslatorRepository
                        .GetAllByCondition(x => x.ProjectAntennaId == projectAntennae[l].Id).ToList();
                    var number = 1;
                    foreach (var antennaTranslator in antennaTranslators)
                        antennaTranslator.TranslatorType = await _repositoryWrapper.TranslatorTypeRepository
                            .GetByCondition(x => x.Id == antennaTranslator.TranslatorTypeId);
                    powerList.AddRange(antennaTranslators.Select(antennaTranslator => antennaTranslator.Power.ToString()));
                    gainList.AddRange(antennaTranslators.Select(antennaTranslator => antennaTranslator.Gain.ToString()));
                    frequencyList.AddRange(antennaTranslators.Select(antennaTranslator => antennaTranslator.TranslatorSpecs.Frequency.ToString()));
                    typeList.AddRange(antennaTranslators.Select(antennaTranslator => antennaTranslator.TranslatorType?.Type ?? ""));
                    tiltList.AddRange(antennaTranslators.Select(antennaTranslator => antennaTranslator.Tilt.ToString()));
                    foreach (var antennaTranslator in antennaTranslators)
                    {
                        antennaTranslatorId = antennaTranslator.Id;
                        countTranslators++;
                        //Горизонтальное
                        var bioHorizontal = _repositoryWrapper.BiohazardRadiusRepository.GetAllByCondition(x =>
                            x.AntennaTranslatorId == antennaTranslator.Id && x.DirectionType == DirectionType.Horizontal.GetDescription()).OrderBy(x=>x.Degree).ToList();
                        var maxHorizontalZ = bioHorizontal.Max(radiation => Math.Abs(radiation.BiohazardRadiusZ));
                        var radiationMaxHorizontalZ = bioHorizontal.First(radiation => radiation.BiohazardRadiusZ == maxHorizontalZ);
                        var horizontalX = radiationMaxHorizontalZ.BiohazardRadiusX;
                        var horizontalBack = await _repositoryWrapper.BiohazardRadiusRepository
                            .GetByCondition(x => x.Degree == 180 && x.DirectionType == DirectionType.Horizontal.GetDescription());
                        var maxMaximumHorizontal = bioHorizontal.Max(radiation => Math.Abs(radiation.MaximumBiohazardRadius));
                        var maxRadiationHorizontal = bioHorizontal.First(radiation => radiation.MaximumBiohazardRadius == maxMaximumHorizontal);
                        //Вертикальное
                        var bioVertical = _repositoryWrapper.BiohazardRadiusRepository.GetAllByCondition(x =>
                            x.AntennaTranslatorId == antennaTranslator.Id && x.DirectionType == DirectionType.Vertical.GetDescription()).OrderBy(x=>x.Degree).ToList();
                        var minVerticalZ = bioVertical.Min(radiation => radiation.BiohazardRadiusZ);
                        var radiationMinVerticalZ = bioVertical
                            .FirstOrDefault(radiation => radiation.BiohazardRadiusZ == minVerticalZ);
                        var verticalX = radiationMinVerticalZ.BiohazardRadiusX;
                        var verticalBack = await _repositoryWrapper.BiohazardRadiusRepository
                            .GetByCondition(x => x.Degree == 180 && x.DirectionType == DirectionType.Vertical.GetDescription());
                        var maxMaximumVertical = bioVertical.Max(radiation => Math.Abs(radiation.MaximumBiohazardRadius));
                        var maxRadiationVertical = bioHorizontal
                            .First(radiation => radiation.MaximumBiohazardRadius == maxMaximumVertical);
                        var maxRadius = Math.Max(maxMaximumHorizontal,maxMaximumVertical);
                        //Количество столбцов таблицы
                        var countTable = CheckCountTable(bioVertical, maxRadiationHorizontal.Degree, radiationMaxHorizontalZ.Degree,DirectionType.Vertical) - 1;
                        //Поиск и создание таблицы для транслятора
                        var keywords = document.FindAll("[Table]",SearchOptions.WholeWord);
                        DocumentPosition insertPosition = keywords[0].Start;
                        ParagraphProperties titleParagraphProperties = document.BeginUpdateParagraphs(keywords[0]);
                        titleParagraphProperties.Alignment = ParagraphAlignment.Center;
                        document.EndUpdateParagraphs(titleParagraphProperties);
                        document.InsertText(insertPosition, $"Владелец радиоэлектронных средств: {contrAgent.CompanyName}\n");
                        document.Delete(keywords[0]);
                        Paragraph newAppendedParagraph = document.Paragraphs.Insert(insertPosition);
                        Table oldTable = document.Tables.Create(newAppendedParagraph.Range.Start, countTable, 12);
                        oldTable.Rows.InsertBefore(0);
                        oldTable.Rows.InsertAfter(0);
                        oldTable.Rows[0].Cells.Append();
                        //Формирование ячеек таблицы для транслятора
                        Table table = document.Tables.Last;
                        table.TableAlignment = TableRowAlignment.Center;
                        table.MergeCells(table[0, 6], table[countTable+1, 6]);
                        table.BeginUpdate();
                        for (int i = 0; i <= 12; i++)
                        {
                            TableCell columnCell = table[i, i];
                            columnCell.PreferredWidthType = WidthType.Auto;
                            for (int j = 0; j <= countTable+1; j++)
                            {
                                columnCell = table[j, i];
                                columnCell.HeightType = HeightType.Auto;
                                columnCell.Height = 0.131f;
                                DocumentRange cellRange = columnCell.Range;
                                CharacterProperties cp = document.BeginUpdateCharacters(cellRange);
                                cp.FontSize = 8;
                                document.EndUpdateCharacters(cp);
                            }
                        }
                        //Заполнение шапки таблицы
                        document.InsertSingleLineText(table[0, 0].Range.Start, "v, град");
                        document.InsertSingleLineText(table[0, 1].Range.Start, "f(v), dBi");
                        document.InsertSingleLineText(table[0, 2].Range.Start, "f(v), раз");
                        document.InsertSingleLineText(table[0, 3].Range.Start, "Rб, м");
                        document.InsertSingleLineText(table[0, 4].Range.Start, "Rz, м");
                        document.InsertSingleLineText(table[0, 5].Range.Start, "Rx, м");
                        document.InsertSingleLineText(table[0, 7].Range.Start, "v, град");
                        document.InsertSingleLineText(table[0, 8].Range.Start, "f(v), dBi");
                        document.InsertSingleLineText(table[0, 9].Range.Start, "f(v), раз");
                        document.InsertSingleLineText(table[0, 10].Range.Start, "Rб, м");
                        document.InsertSingleLineText(table[0, 11].Range.Start, "Rz, м");
                        document.InsertSingleLineText(table[0, 12].Range.Start, "Rx, м");
                        //Вызов метода заполнения таблиц 360 для транслятора
                        CreateTable360(document, table, bioVertical,maxRadiationHorizontal.Degree,radiationMaxHorizontalZ.Degree,DirectionType.Vertical);
                        CreateTable360(document, table, bioHorizontal,maxRadiationVertical.Degree,radiationMinVerticalZ.Degree,DirectionType.Horizontal);
                        //Формирования текста под таблицей
                        Paragraph newMaxAppendedParagraphText = document.Paragraphs.Insert(table.Range.End);
                        CharacterProperties cpFirst = document.BeginUpdateCharacters(newMaxAppendedParagraphText.Range);
                        cpFirst.FontSize = 8;
                        cpFirst.FontName = "Cambria Math";
                        document.EndUpdateCharacters(cpFirst);
                        document.InsertText(newMaxAppendedParagraphText.Range.Start,"Максимальный радиус биологически-опасной зоны от секторных " +
                                                                                 $"антенн {antennaTranslator.ProjectAntenna.Antenna.Model} в направлении излучения равен " +
                                                                                 $"{maxRadius.ToString("F3")} м" +
                                                                                 $" (стандарт {antennaTranslator.TranslatorType.Type}; мощность передатчика {antennaTranslator.Power} Вт; " +
                                                                                 $"частота на передачу {antennaTranslator.TranslatorSpecs.Frequency} МГц;" +
                                                                                 $" коэффициент усиления антенн {antennaTranslator.Gain} дБ, " +
                                                                                 $"направление антенны в вертикальной плоскости " +
                                                                                 $"{antennaTranslator.ProjectAntenna.Tilt}°).\n " +
                                                                                 "В вертикальном сечении БОЗ повторяет диаграмму направленности." +
                                                                                 " Максимальное отклонение от оси в вертикальном сечении составляет " +
                                                                                 $"{(Math.Abs(minVerticalZ)).ToString("F3")} м." +
                                                                                 $" на расстоянии {verticalX.ToString("F3")} м. от центра излучения. " +
                                                                                 "Максимальный радиус биологически-опасного излучения " +
                                                                                 "от заднего лепестка антенны составил " +
                                                                                 $"{verticalBack.MaximumBiohazardRadius.ToString("F3")} м.\n " +
                                                                                 "В горизонтальном сечении БОЗ повторяет диаграмму направленности. " +
                                                                                 "Максимальное отклонение от оси в горизонтальном сечении составляет " +
                                                                                 $"{maxHorizontalZ.ToString("F3")} м." +
                                                                                 $" на расстоянии {horizontalX.ToString("F3")} м. от центра излучения. " +
                                                                                 "Максимальный радиус биологически-опасного излучения от" +
                                                                                 $" заднего лепестка антенны составил {horizontalBack.MaximumBiohazardRadius.ToString("F3")} м.");
                        table.EndUpdate();
                        //Создание новой страницы с таблицей и диаграммами направлености формирование самой таблицы
                        var secondSection = document.AppendSection();
                        Table oldTableSecond = document.Tables.Create(secondSection.Range.Start, 5, 2);
                        oldTableSecond.Rows[0].Cells.Append();
                        Table tableSecond = document.Tables.Last;
                        tableSecond.TableAlignment = TableRowAlignment.Center;
                        tableSecond.BeginUpdate();
                        for (int i = 0; i <= 2; i++)
                        {
                            TableCell columnCell = tableSecond[i, i];
                            columnCell.PreferredWidthType = WidthType.Auto;
                            for (int j = 0; j < 5; j++)
                            {
                                columnCell = tableSecond[j, i];
                                columnCell.HeightType = HeightType.Auto;
                                columnCell.Height = 0.250f;
                                DocumentRange cellRange = columnCell.Range;
                                CharacterProperties cp = document.BeginUpdateCharacters(cellRange);
                                cp.FontSize = 12;
                                cp.FontName = "Cambria Math";
                                document.EndUpdateCharacters(cp);
                            }
                        }
                        //Заполнение таблицы данными
                        document.InsertSingleLineText(tableSecond[0, 0].Range.Start, "Расчет биологически опасной зоны от секторной антенны:");
                        document.InsertSingleLineText(tableSecond[1, 0].Range.Start, "Рабочая частота (диапазон частот) на передачу, МГц чу, Вт:");
                        document.InsertSingleLineText(tableSecond[2, 0].Range.Start, "Мощность на передачу, Вт:");
                        document.InsertSingleLineText(tableSecond[3, 0].Range.Start, "Коэффициент усиления антенн, дБ");
                        document.InsertSingleLineText(tableSecond[4, 0].Range.Start, "Стандарт:");
                        document.InsertSingleLineText(tableSecond[0, 1].Range.Start, $"{antennaTranslator.ProjectAntenna.Antenna.Model}");
                        document.InsertSingleLineText(tableSecond[1, 1].Range.Start, $"{antennaTranslator.TranslatorSpecs.Frequency}");
                        document.InsertSingleLineText(tableSecond[2, 1].Range.Start, $"{antennaTranslator.Power}");
                        document.InsertSingleLineText(tableSecond[3, 1].Range.Start, $"{antennaTranslator.Gain}");
                        document.InsertSingleLineText(tableSecond[4, 1].Range.Start, $"{antennaTranslator.TranslatorType.Type}");
                        document.InsertSingleLineText(tableSecond[0, 2].Range.Start, $"Владелец радиоэлектронных средств: {contrAgent.CompanyName}");
                        document.InsertSingleLineText(tableSecond[1, 2].Range.Start, $"Угол наклона антенны и передатчика " +
                            $"{antennaTranslator.ProjectAntenna.Tilt}°({antennaTranslator.Tilt}°)");
                        document.InsertSingleLineText(tableSecond[2, 2].Range.Start, $"Передатчик №{number}");
                        tableSecond.MergeCells(tableSecond[0, 0], tableSecond[4, 0]);
                        tableSecond.MergeCells(tableSecond[0, 1], tableSecond[4, 1]);
                        tableSecond.MergeCells(tableSecond[0, 2], tableSecond[4, 2]);
                        tableSecond.EndUpdate();
                        //Формирование текста под таблицей
                        ParagraphProperties paragraphProperties = document.BeginUpdateParagraphs(secondSection.Range);
                        paragraphProperties.Alignment = ParagraphAlignment.Center;
                        CharacterProperties cpSecond = document.BeginUpdateCharacters(secondSection.Range);
                        cpSecond.FontSize = 11;
                        cpSecond.FontName = "Cambria Math";
                        document.EndUpdateCharacters(cpSecond);
                        document.EndUpdateParagraphs(paragraphProperties);
                        document.InsertText(secondSection.Range.End,"\nРасчеты размеров БОЗ в вертикальной и горизонтальной плоскостях:\n" +
                                                                    "Биологически-опасная зона антенны повторяет форму диаграммы направленности в " +
                                                                    "горизонтальной и вертикальной плоскости.\n" +
                                                                    "Максимальный радиус биологически опасной зоны, Rб, м, в " +
                                                                    "направлении излучения определяется по формуле:\n" +
                                                                    "Rб = [(8*P* G( 𝜽 )*K* η)/ Ппду]^0,5 * F( 𝜽 ) * F( 𝝋 )\n" +
                                                                    "Для определения максимального радиуса БОЗ примем F(𝜃)=1 и F(𝜑)=1:\n" +
                                                                    $"Максимальный радиус БОЗ составляет Rmax= {maxRadius} м.\n" +
                                                                    "Форму поперечного сечения биологически опасной зоны рассчитаем при помощи формул:\n" +
                                                                    "Rz=Rmax•sin 𝝋, Rx=Rmax•cos 𝝋.                                        " +
                                                                    "Rz=Rmax•sin 𝜃, Rx=Rmax•cos 𝜃 \n" +
                                                                    "для горизонтальной плоскости                                               " +
                                                                    "для вертикальной плоскости \n" +
                                                                    "Значение Rz указывает на отклонение БОЗ от оси излучения антенны," +
                                                                    " перпендикулярно к ней на расстоянии Rx от центра излучения вдоль оси");
                        //Вызов методов отрисовки диаграмм
                        CreateDiagram(document,secondSection.Range.End,minVerticalZ,bioHorizontal);
                        CreateDiagram(document,secondSection.Range.End,maxHorizontalZ,bioVertical);
                        //Создание новой страницы с ключом
                        var thirdSection = document.InsertSection(secondSection.Range.End);
                        document.InsertText(thirdSection.Range.End,"[Table]");
                        number++;
                        table.EndUpdate();
                    }
                    //Получение суммарных и максимальных данных по трансляторам
                    var summary = _repositoryWrapper.SummaryBiohazardRadiusRepository
                        .GetAllByCondition(x => x.AntennaTranslatorId == antennaTranslatorId);
                    //Получение горизонтальных данных
                    var horizontalSummary =  summary.Where(x => x.DirectionType == DirectionType.Horizontal)
                        .OrderBy(x=>x.Degree).ToList();
                    var maxHorizontalSummaryZ = horizontalSummary.Max(x => Math.Abs(x.BiohazardRadiusZ));
                    var radiationMaxHorizontalSummaryZ = horizontalSummary
                        .Where(x => Math.Abs(x.BiohazardRadiusZ) == maxHorizontalSummaryZ).FirstOrDefault();
                    var horizontalSummaryX = radiationMaxHorizontalSummaryZ.BiohazardRadiusX;
                    var horizontalSummaryBack = await _repositoryWrapper.SummaryBiohazardRadiusRepository.GetByCondition
                        (x => x.Degree == 180 && x.DirectionType == DirectionType.Horizontal && x.AntennaTranslatorId == antennaTranslatorId);
                    var maxMaximumHorizontalSummary = horizontalSummary.Max(x => Math.Abs(x.MaximumBiohazardRadius));
                    var maxRadiationHorizontalSummary = horizontalSummary
                        .First(x => x.MaximumBiohazardRadius == maxMaximumHorizontalSummary);
                    //Получение вертикальных данных
                    var verticalSummary = summary.Where(x => x.DirectionType == DirectionType.Vertical)
                        .OrderBy(x=>x.Degree).ToList();
                    var minVerticalSummaryZ = verticalSummary.Min(x => x.BiohazardRadiusZ);
                    var radiationMinVerticalSummaryZ = verticalSummary.First(x => x.BiohazardRadiusZ == minVerticalSummaryZ);
                    var verticalSummaryX = radiationMinVerticalSummaryZ.BiohazardRadiusX;
                    var verticalSummaryBack = await _repositoryWrapper.SummaryBiohazardRadiusRepository.GetByCondition
                        (x => x.Degree == 180 && x.DirectionType == DirectionType.Vertical && x.AntennaTranslatorId == antennaTranslatorId);
                    var maxMaximumVerticalSummary = verticalSummary.Max(x => Math.Abs(x.MaximumBiohazardRadius));
                    var maxRadiationVerticalSummary = verticalSummary
                        .First(x => x.MaximumBiohazardRadius == maxMaximumVerticalSummary);
                    var maxSummaryRadius = Math.Max(maxMaximumHorizontalSummary,maxMaximumVerticalSummary);
                    var maxCountTable = CheckMaxCountTable(verticalSummary, 
                            maxRadiationHorizontalSummary.Degree, radiationMaxHorizontalSummaryZ.Degree,DirectionType.Vertical) - 1;
                    if (antennaTranslators.Count > 1)
                    {
                        //Формирование таблицы для суммарных и максимальных данных
                        var keywordsMax = document.FindAll("[Table]",SearchOptions.WholeWord);
                        ParagraphProperties maxParagraphProperties = document.BeginUpdateParagraphs(keywordsMax[0]);
                        maxParagraphProperties.Alignment = ParagraphAlignment.Center;
                        document.EndUpdateParagraphs(maxParagraphProperties);
                        document.InsertText(keywordsMax[0].Start, $"Владелец радиоэлектронных средств: {contrAgent.CompanyName}\n");
                        document.Delete(keywordsMax[0]);
                        Paragraph maxAppendedParagraph = document.Paragraphs.Insert(keywordsMax[0].End);
                        Table newMaxTable = document.Tables.Create(maxAppendedParagraph.Range.Start, maxCountTable, 8);
                        newMaxTable.Rows.InsertBefore(0);
                        newMaxTable.Rows.InsertAfter(0);
                        newMaxTable.Rows[0].Cells.Append();
                        Table maxTable = document.Tables.Last;
                        maxTable.TableAlignment = TableRowAlignment.Center;
                        maxTable.MergeCells(maxTable[0, 4], maxTable[maxCountTable+1, 4]);
                        maxTable.BeginUpdate();
                        //Формирование ячеек для суммарных и максимальных данных
                        for (int i = 0; i <= 8; i++)
                        {
                            TableCell columnCellMax = maxTable[i, i];
                            columnCellMax.PreferredWidthType = WidthType.Auto;
                            for (int j = 0; j <= maxCountTable+1; j++)
                            {
                                columnCellMax = maxTable[j, i];
                                columnCellMax.HeightType = HeightType.Auto;
                                columnCellMax.Height = 0.131f;
                                CharacterProperties cpMax = document.BeginUpdateCharacters(columnCellMax.Range);
                                cpMax.FontSize = 8;
                                document.EndUpdateCharacters(cpMax);
                            }
                        }
                        //Заполнение шабки таблицы данными
                        document.InsertSingleLineText(maxTable[0, 0].Range.Start, "v, град");
                        document.InsertSingleLineText(maxTable[0, 1].Range.Start, "Rб, м");
                        document.InsertSingleLineText(maxTable[0, 2].Range.Start, "Rz, м");
                        document.InsertSingleLineText(maxTable[0, 3].Range.Start, "Rx, м");
                        document.InsertSingleLineText(maxTable[0, 5].Range.Start, "v, град");
                        document.InsertSingleLineText(maxTable[0, 6].Range.Start, "Rб, м");
                        document.InsertSingleLineText(maxTable[0, 7].Range.Start, "Rz, м");
                        document.InsertSingleLineText(maxTable[0, 8].Range.Start, "Rx, м");
                        //Вызов методов заполнения таблиц данными
                        CreateTableMaximum360(document, maxTable, verticalSummary,maxRadiationHorizontalSummary.Degree,
                            radiationMaxHorizontalSummaryZ.Degree,DirectionType.Vertical);
                        CreateTableMaximum360(document, maxTable, horizontalSummary,maxRadiationVerticalSummary.Degree,
                            radiationMinVerticalSummaryZ.Degree,DirectionType.Horizontal);
                        //Формирование текста под таблицами
                        Paragraph newAppendedParagraphText = document.Paragraphs.Insert(maxTable.Range.End);
                        document.BeginUpdateParagraphs(newAppendedParagraphText.Range);
                        CharacterProperties cpNew = document.BeginUpdateCharacters(newAppendedParagraphText.Range);
                        cpNew.FontSize = 8;
                        cpNew.FontName = "Cambria Math";
                        document.EndUpdateCharacters(cpNew);
                        document.InsertText(newAppendedParagraphText.Range.Start,"Максимальный радиус биологически-опасной зоны от секторных " +
                                                                                 $"антенн {projectAntennae[l].Antenna.Model} в направлении излучения равен " +
                                                                                 $"{maxSummaryRadius.ToString("F3")} м" +
                                                                                 $" (стандарт {type}; мощность передатчика {power} Вт; " +
                                                                                 $"частота на передачу {frequency} МГц;" +
                                                                                 $" коэффициент усиления антенн {gain} дБ, " +
                                                                                 $"направление антенны в вертикальной плоскости " +
                                                                                 $"{projectAntennae[l].Tilt}°).\n " +
                                                                                 "В вертикальном сечении БОЗ повторяет диаграмму направленности." +
                                                                                 " Максимальное отклонение от оси в вертикальном сечении составляет " +
                                                                                 $"{(Math.Abs(minVerticalSummaryZ)).ToString("F3")} м." +
                                                                                 $" на расстоянии {verticalSummaryX.ToString("F3")} м. от центра излучения. " +
                                                                                 "Максимальный радиус биологически-опасного излучения " +
                                                                                 "от заднего лепестка антенны составил " +
                                                                                 $"{verticalSummaryBack.MaximumBiohazardRadius.ToString("F3")} м.\n " +
                                                                                 "В горизонтальном сечении БОЗ повторяет диаграмму направленности. " +
                                                                                 "Максимальное отклонение от оси в горизонтальном сечении составляет " +
                                                                                 $"{maxHorizontalSummaryZ.ToString("F3")} м." +
                                                                                 $" на расстоянии {horizontalSummaryX.ToString("F3")} м. от центра излучения. " +
                                                                                 "Максимальный радиус биологически-опасного излучения от" +
                                                                                 $" заднего лепестка антенны составил " +
                                                                                 $"{horizontalSummaryBack.MaximumBiohazardRadius.ToString("F3")} м.");
                        maxTable.EndUpdate();
                        //Формирование страницы с таблицей и диаграммами направленности
                        var secondSectionMax = document.AppendSection();
                        Table oldTableSecondMax = document.Tables.Create(secondSectionMax.Range.Start, 5, 2);
                        oldTableSecondMax.Rows[0].Cells.Append();
                        Table tableSecondMax = document.Tables.Last;
                        tableSecondMax.TableAlignment = TableRowAlignment.Center;
                        tableSecondMax.BeginUpdate();
                        //Формирование ячеек
                        for (int i = 0; i <= 2; i++)
                        {
                            TableCell columnCellMax = tableSecondMax[i, i];
                            columnCellMax.PreferredWidthType = WidthType.Auto;
                            for (int j = 0; j < 5; j++)
                            {
                                columnCellMax = tableSecondMax[j, i];
                                columnCellMax.HeightType = HeightType.Auto;
                                columnCellMax.Height = 0.250f;
                                CharacterProperties cpMax = document.BeginUpdateCharacters(columnCellMax.Range);
                                cpMax.FontSize = 12;
                                cpMax.FontName = "Cambria Math";
                                document.EndUpdateCharacters(cpMax);
                            }
                        }
                        //Сбор всех данных по транслятором относящимся к антенне
                        gain = string.Join(";", gainList);
                        type = string.Join(";", typeList);
                        power = string.Join(";", powerList);
                        frequency = string.Join(";", frequencyList);
                        tilt = string.Join(";", tiltList);
                        //Заполнение таблицы данными
                        document.InsertSingleLineText(tableSecondMax[0, 0].Range.Start, "Расчет биологически опасной зоны от секторной антенны:");
                        document.InsertSingleLineText(tableSecondMax[1, 0].Range.Start, "Рабочая частота (диапазон частот) на передачу, МГц чу, Вт:");
                        document.InsertSingleLineText(tableSecondMax[2, 0].Range.Start, "Мощность на передачу, Вт:");
                        document.InsertSingleLineText(tableSecondMax[3, 0].Range.Start, "Коэффициент усиления антенн, дБ");
                        document.InsertSingleLineText(tableSecondMax[4, 0].Range.Start, "Стандарт:");
                        document.InsertSingleLineText(tableSecondMax[0, 1].Range.Start, $"{projectAntennae[l].Antenna.Model}");
                        document.InsertSingleLineText(tableSecondMax[1, 1].Range.Start, $"{frequency}");
                        document.InsertSingleLineText(tableSecondMax[2, 1].Range.Start, $"{power}");
                        document.InsertSingleLineText(tableSecondMax[3, 1].Range.Start, $"{gain}");
                        document.InsertSingleLineText(tableSecondMax[4, 1].Range.Start, $"{type}");
                        document.InsertSingleLineText(tableSecondMax[0, 2].Range.Start, $"Владелец радиоэлектронных средств: {contrAgent.CompanyName}");
                        document.InsertSingleLineText(tableSecondMax[1, 2].Range.Start, $"Угол наклона антенны и передатчиков {projectAntennae[l].Tilt}°({tilt}°)");
                        tableSecondMax.MergeCells(tableSecondMax[0, 0], tableSecondMax[4, 0]);
                        tableSecondMax.MergeCells(tableSecondMax[0, 1], tableSecondMax[4, 1]);
                        tableSecondMax.MergeCells(tableSecondMax[0, 2], tableSecondMax[4, 2]);
                        tableSecondMax.EndUpdate();
                        //Формирование текста под таблицей
                        ParagraphProperties paragraphPropertiesMax = document.BeginUpdateParagraphs(secondSectionMax.Range);
                        paragraphPropertiesMax.Alignment = ParagraphAlignment.Center;
                        CharacterProperties cpSecondMax = document.BeginUpdateCharacters(secondSectionMax.Range);
                        cpSecondMax.FontSize = 11;
                        cpSecondMax.FontName = "Cambria Math";
                        document.EndUpdateCharacters(cpSecondMax);
                        document.EndUpdateParagraphs(paragraphPropertiesMax);
                        var formula = string.Join(" + ", Enumerable.Range(1, antennaTranslators.Count).Select(i => $"𝑅𝑅б{i}²"));
                        document.InsertText(secondSectionMax.Range.End,"\nСуммируем результаты радиуса биологически-опасной зоны от передатчиков" +
                                                                       " секторной антенны в вертикальной и горизонтальной плоскостях для определения" +
                                                                       $" максимального радиус биологически-опасной зоны от секторной антенны: \nRб =√{formula}");
                        //Вызов методов формирование диаграмм
                        CreateDiagramSummary(document,secondSectionMax.Range.End,minVerticalSummaryZ,horizontalSummary);
                        CreateDiagramSummary(document,secondSectionMax.Range.End,maxHorizontalSummaryZ,verticalSummary);
                        //Создание новой страницы с ключем
                        var thirdSectionMax = document.InsertSection(secondSectionMax.Range.End);
                        document.InsertText(thirdSectionMax.Range.End,"[Table]");
                        tableSecondMax.EndUpdate();
                    }
                    //Заполнение вывода данными по антеннам
                    var antennae = document.FindAll("[Antennae]",SearchOptions.WholeWord);
                    document.InsertText(antennae[0].Start, $"Антенна {projectAntennae[l].Antenna.Model} (сектор {l+1} – " +
                                                    $"количество передатчиков {antennaTranslators.Count} шт.) " +
                                                    $"Антенны размещаются на {project.PlaceOfInstall.ToLower()}, на высоте {projectAntennae[l].HeightFromGroundLevel} м. " +
                                                    $"Частота передачи {frequency} МГц. Коэффициент усиления {gain} дБ. " +
                                                    $"Мощность передатчиков {power} Вт. Максимальный радиус биологически-опасной зоны от секторных антенн" +
                                                    $" {projectAntennae[l].Antenna.Model} в направлении излучения равен {maxSummaryRadius.ToString("F3")} м " +
                                                    $"(угол наклона антенны {projectAntennae[l].Tilt}°,угол наклона передатчиков {tilt}°). " +
                                                    $"В вертикальном сечении БОЗ повторяет диаграмму направленности." +
                                                    $" Максимальное отклонение от оси в вертикальном сечении составляет {(Math.Abs(minVerticalSummaryZ)).ToString("F3")} м. " +
                                                    $"на расстоянии {verticalSummaryX.ToString("F3")} м. от центра излучения." +
                                                    $" Максимальный радиус биологически-опасного излучения от заднего лепестка антенны составил " +
                                                    $"{verticalSummaryBack.MaximumBiohazardRadius.ToString("F3")} м." +
                                                    $" В горизонтальном сечении БОЗ повторяет диаграмму направленности." +
                                                    $" Максимальное отклонение от оси в горизонтальном сечении составляет {maxHorizontalSummaryZ.ToString("F3")} м. " +
                                                    $"на расстоянии {horizontalSummaryX.ToString("F3")} м. от центра излучения. " +
                                                    $"Максимальный радиус биологически-опасного излучения от заднего лепестка антенны составил " +
                                                    $"{horizontalSummaryBack.MaximumBiohazardRadius.ToString("F3")} м.");
                    document.Delete(antennae[0]);
                    minVerticalSummaryZ = Math.Abs(minVerticalSummaryZ);
                    var height = projectAntennae[l].HeightFromGroundLevel - minVerticalSummaryZ;
                    var azimut = document.FindAll("[Azimut]",SearchOptions.WholeWord);
                    var azimutText = $"Зона ограничения застройки повторяет форму биологически-опасной зоны " +
                                     $"и устанавливается на высоте {height.ToString("F3")} м от земли, не задевая существующие здания: " +
                                     $"\nв направлении {projectAntennae[l].Azimuth}°; на расстоянии {maxSummaryRadius.ToString("F3")} м от антенны.";
                    if (l != projectAntennae.Count - 1)
                    {
                        document.InsertText(antennae[0].End, $"\n[Antennae]");
                        azimutText = $"Зона ограничения застройки повторяет форму биологически-опасной зоны " +
                                     $"и устанавливается на высоте {height.ToString("F3")} м от земли, не задевая существующие здания: " +
                                     $"\nв направлении {projectAntennae[l].Azimuth}°; на расстоянии {maxSummaryRadius.ToString("F3")} м от антенны.\n[Azimut]";
                    }
                    document.InsertText(azimut[0].Start, azimutText);
                    document.Delete(azimut[0]);
                }
                var firstTable = document.FindAll("[FirstTable]",SearchOptions.WholeWord);
                DocumentPosition firstTablePosition = firstTable[0].Start;
                ParagraphProperties firstTableParagraphProperties = document.BeginUpdateParagraphs(firstTable[0]);
                firstTableParagraphProperties.Alignment = ParagraphAlignment.Left;
                document.EndUpdateParagraphs(firstTableParagraphProperties);
                document.Delete(firstTable[0]);
                Paragraph newFirstTableAppendedParagraph = document.Paragraphs.Insert(firstTablePosition);
                Table oldFirstTable = document.Tables.Create(newFirstTableAppendedParagraph.Range.Start, 13, countTranslators+1);
                oldFirstTable.Rows.InsertBefore(0);
                oldFirstTable.Rows.InsertAfter(0);
                oldFirstTable.Rows[0].Cells.Append();
                //Формирование ячеек таблицы для транслятора
                Table tableFirst = document.Tables.Last;
                tableFirst.TableAlignment = TableRowAlignment.Left;
                tableFirst.MergeCells(tableFirst[2, 0], tableFirst[3, 0]);
                tableFirst.MergeCells(tableFirst[4, 0], tableFirst[5, 0]);
                tableFirst.MergeCells(tableFirst[6, 0], tableFirst[7, 0]);
                tableFirst.MergeCells(tableFirst[13, 0], tableFirst[14, 0]);
                tableFirst.MergeCells(tableFirst[2, 1], tableFirst[3, 1]);
                tableFirst.MergeCells(tableFirst[4, 1], tableFirst[5, 1]);
                tableFirst.MergeCells(tableFirst[6, 1], tableFirst[7, 1]);
                tableFirst.MergeCells(tableFirst[13, 1], tableFirst[14, 1]);
                tableFirst.BeginUpdate();
                document.InsertSingleLineText(tableFirst[0, 1].Range.Start, "Антенна");
                document.InsertSingleLineText(tableFirst[1, 1].Range.Start, "Мощность передающего радиоэлектронного средства в Вт");
                document.InsertSingleLineText(tableFirst[2, 1].Range.Start, "Рабочая частота (диапазон частот) на передачу, МГц");
                document.InsertSingleLineText(tableFirst[4, 1].Range.Start, "Коэффициент усиления антенны (дБ/раз)");
                document.InsertSingleLineText(tableFirst[6, 1].Range.Start, "Потери мощности в антенно-фидерном тракте на передачу (дБ/раз)" +
                                                                            " (если данных нет, то указать длину фидера (кабеля от передатчика до антенны), м;" +
                                                                            " и потери мощности в фидере (дБ/метр)");
                document.InsertSingleLineText(tableFirst[8, 1].Range.Start, "Вертикальный размер или диаметр антенны");
                document.InsertSingleLineText(tableFirst[9, 1].Range.Start, "Угол места основного лепестка в градусах, " +
                                                                            "(угол направления максимального излучения антенны в вертикальной плоскости)");
                document.InsertSingleLineText(tableFirst[10, 1].Range.Start, "Азимут максимума излучения (для антенн кругового действия 0-360°)");
                document.InsertSingleLineText(tableFirst[11, 1].Range.Start, "Режим работы РТО на излучение (постоянный, повторно-кратковременный, импульсный)");
                document.InsertSingleLineText(tableFirst[12, 1].Range.Start, "Место и тип размещения антенны " +
                                                                             "(например, \"на крыше АБК\", \"на кронштейне на стене технического этажа\")");
                document.InsertSingleLineText(tableFirst[13, 1].Range.Start, "Высота подвеса антенны в метрах, м: " +
                                                                             " \n-от уровня земли (указывается высота размещения фазового центра каждой антенны);" +
                                                                             " \n-от уровня крыши (указывается от крыши, где установлена антенна," +
                                                                             " или от уровня крыши ближайшего наиболее высокого здания");
                var x = 2;
                for (int i = 0; i <= 14; i++)
                {
                    if (i < 2)
                        document.InsertSingleLineText(tableFirst[i, 0].Range.Start, "1");
                    else
                    {
                        document.InsertSingleLineText(tableFirst[i, 0].Range.Start, $"{x}");
                        if (i == 2 || i == 4 || i == 6 || i == 13)
                            i++;
                        if (x < 10)
                            x++;
                    }
                }
                for (int i = 0; i < projectAntennae.Count; i++)
                {
                    var antennaTranslators = _repositoryWrapper.AntennaTranslatorRepository.GetAllByCondition(x =>
                        x.ProjectAntennaId == projectAntennae[i].Id).ToList();
                    tableFirst.MergeCells(tableFirst[0, i + 2], tableFirst[0, i + antennaTranslators.Count + 1]);
                    tableFirst.MergeCells(tableFirst[8, i + 2], tableFirst[8, i + antennaTranslators.Count + 1]);
                    tableFirst.MergeCells(tableFirst[9, i + 2], tableFirst[9, i + antennaTranslators.Count + 1]);
                    tableFirst.MergeCells(tableFirst[10, i + 2], tableFirst[10, i + antennaTranslators.Count + 1]);
                    tableFirst.MergeCells(tableFirst[11, i + 2], tableFirst[11, i + antennaTranslators.Count + 1]);
                    tableFirst.MergeCells(tableFirst[12, i + 2], tableFirst[12, i + antennaTranslators.Count + 1]);
                    tableFirst.MergeCells(tableFirst[13, i + 2], tableFirst[13, i + antennaTranslators.Count + 1]);
                    tableFirst.MergeCells(tableFirst[14, i + 2], tableFirst[14, i + antennaTranslators.Count + 1]);
                    document.InsertSingleLineText(tableFirst[0, i + 2].Range.Start, projectAntennae[i].Antenna.Model);
                    document.InsertSingleLineText(tableFirst[8, i + 2].Range.Start, projectAntennae[i].Antenna.VerticalSizeDiameter.ToString());
                    document.InsertSingleLineText(tableFirst[9, i + 2].Range.Start, projectAntennae[i].Tilt.ToString());
                    document.InsertSingleLineText(tableFirst[10, i + 2].Range.Start, projectAntennae[i].Azimuth.ToString());
                    document.InsertSingleLineText(tableFirst[11, i + 2].Range.Start, projectAntennae[i].RtoRadiationMode);
                    document.InsertSingleLineText(tableFirst[12, i + 2].Range.Start, project.PlaceOfInstall);
                    document.InsertSingleLineText(tableFirst[13, i + 2].Range.Start, projectAntennae[i].HeightFromGroundLevel.ToString());
                    document.InsertSingleLineText(tableFirst[14, i + 2].Range.Start, projectAntennae[i].HeightFromRoofLevel.ToString());
                    tableFirst.BeginUpdate();
                }
                
                for (int i = 0; i <= countTranslators+1; i++)
                {
                    TableCell columnFirstTableCell = tableFirst[1, i];
                    columnFirstTableCell.PreferredWidthType = WidthType.Fixed;
                    if (i == 0)
                        columnFirstTableCell.PreferredWidth = 0.25f;
                    else if (i == 1)
                        columnFirstTableCell.PreferredWidth = 4.35f;
                    else
                        columnFirstTableCell.PreferredWidth = 2.5f;
                    for (int j = 0; j <= 14; j++)
                    {
                        columnFirstTableCell = tableFirst[j, 0];
                        columnFirstTableCell.HeightType = HeightType.Exact;
                        if (j == 0 || j == 2 || j == 6 || j == 9 || j == 12)
                            columnFirstTableCell.Height = 0.28f;
                        else if(j == 13)
                            columnFirstTableCell.Height = 0.42f;
                        else
                            columnFirstTableCell.Height = 0.17f;
                        columnFirstTableCell.VerticalAlignment = TableCellVerticalAlignment.Center;
                        CharacterProperties cpTableFirst = document.BeginUpdateCharacters(columnFirstTableCell.Range);
                        cpTableFirst.FontSize = 8;
                        document.EndUpdateCharacters(cpTableFirst);
                    }
                }
                var flow = document.FindAll("[FlowTable]",SearchOptions.WholeWord);
                DocumentPosition flowPosition = flow[0].Start;
                ParagraphProperties flowParagraphProperties = document.BeginUpdateParagraphs(flow[0]);
                flowParagraphProperties.Alignment = ParagraphAlignment.Center;
                document.EndUpdateParagraphs(flowParagraphProperties);
                document.Delete(flow[0]);
                Paragraph newFlowAppendedParagraph = document.Paragraphs.Insert(flowPosition);
                Table oldFlowTable = document.Tables.Create(newFlowAppendedParagraph.Range.Start, countTranslators, 9);
                oldFlowTable.Rows.InsertBefore(0);
                oldFlowTable.Rows.InsertAfter(0);
                oldFlowTable.Rows[0].Cells.Append();
                //Формирование ячеек таблицы для транслятора
                Table tableFlow = document.Tables.Last;
                tableFlow.TableAlignment = TableRowAlignment.Left;
                tableFlow.HorizontalAlignment = TableHorizontalAlignment.Center;
                tableFlow.BeginUpdate();
                for (int i = 0; i <= 9; i++)
                {
                    TableCell columnFlowCell = tableFlow[0, i];
                    columnFlowCell.PreferredWidthType = WidthType.Fixed;
                    if (i == 0)
                        columnFlowCell.PreferredWidth = 0.35f;
                    else if (i == 1)
                        columnFlowCell.PreferredWidth = 4.35f;
                    else
                        columnFlowCell.PreferredWidth = 1.1f;
                    for (int j = 0; j <= countTranslators+1; j++)
                    {
                        tableFlow.FirstRow.FirstCell.PreferredWidth = 0.5f;
                        columnFlowCell = tableFlow[j, i];
                        columnFlowCell.HeightType = HeightType.Exact;
                        columnFlowCell.Height = 0.20f;
                        columnFlowCell.VerticalAlignment = TableCellVerticalAlignment.Center;
                        CharacterProperties cpFlow = document.BeginUpdateCharacters(columnFlowCell.Range);
                        cpFlow.FontSize = 8;
                        document.EndUpdateCharacters(cpFlow);
                    }
                }
                document.InsertSingleLineText(tableFlow[0, 0].Range.Start, "№");
                document.InsertSingleLineText(tableFlow[0, 1].Range.Start, "Антенна");
                document.InsertSingleLineText(tableFlow[0, 2].Range.Start, "5");
                document.InsertSingleLineText(tableFlow[0, 3].Range.Start, "10");
                document.InsertSingleLineText(tableFlow[0, 4].Range.Start, "20");
                document.InsertSingleLineText(tableFlow[0, 5].Range.Start, "30");
                document.InsertSingleLineText(tableFlow[0, 6].Range.Start, "40");
                document.InsertSingleLineText(tableFlow[0, 7].Range.Start, "60");
                document.InsertSingleLineText(tableFlow[0, 8].Range.Start, "80");
                document.InsertSingleLineText(tableFlow[0, 9].Range.Start, "100");
                document.InsertSingleLineText(tableFlow[countTranslators+1, 1].Range.Start, "∑ППЭ");
                tableFlow.BeginUpdate();
                var positionTableAntenna = 0;
                var positionTableTranslator = 0;
                var allEnergyResults = new List<EnergyResult>();
                for (int i = 0; i < projectAntennae.Count; i++)
                {
                    positionTableAntenna++;
                    var antennaTranslators = _repositoryWrapper.AntennaTranslatorRepository.GetAllByCondition(x =>
                            x.ProjectAntennaId == projectAntennae[i].Id).ToList();
                    tableFlow.MergeCells(tableFlow[positionTableTranslator+1, 1], tableFlow[positionTableTranslator+antennaTranslators.Count, 1]);
                    tableFlow.MergeCells(tableFlow[positionTableTranslator+1, 0], tableFlow[positionTableTranslator+antennaTranslators.Count, 0]);
                    document.InsertSingleLineText(tableFlow[positionTableTranslator+1, 1].Range.Start, projectAntennae[i].Antenna.Model);
                    document.InsertSingleLineText(tableFlow[positionTableTranslator+1, 0].Range.Start, positionTableAntenna.ToString());
                    for (int j = 0; j < antennaTranslators.Count; j++)
                    {
                        var energyResults = _repositoryWrapper.EnergyFlowRepository
                            .GetAllByCondition(x => x.AntennaTranslatorId == antennaTranslators[j].Id).OrderBy(x=>x.Distance).ToList();
                        allEnergyResults.AddRange(energyResults);
                        positionTableTranslator++;
                        CreateTableEnergyResult(document,tableFlow,energyResults,positionTableTranslator);
                        document.InsertSingleLineText(tableFirst[1, positionTableTranslator+1].Range.Start, antennaTranslators[j].Power.ToString());
                        document.InsertSingleLineText(tableFirst[2, positionTableTranslator+1].Range.Start, antennaTranslators[j].TranslatorType.Type);
                        document.InsertSingleLineText(tableFirst[3, positionTableTranslator+1].Range.Start, antennaTranslators[j].TranslatorSpecs.Frequency.ToString());
                        document.InsertSingleLineText(tableFirst[4, positionTableTranslator+1].Range.Start, antennaTranslators[j].Gain.ToString());
                        document.InsertSingleLineText(tableFirst[5, positionTableTranslator+1].Range.Start, 
                            _biohazardRadiusService.Multiplier(antennaTranslators[j].Gain).ToString("F3"));
                        document.InsertSingleLineText(tableFirst[6, positionTableTranslator+1].Range.Start, antennaTranslators[j].TransmitLossFactor.ToString());
                        document.InsertSingleLineText(tableFirst[7, positionTableTranslator+1].Range.Start, 
                            Math.Pow(10, -(double)antennaTranslators[j].TransmitLossFactor / 10).ToString("F3"));
                    }
                }
                tableFlow.BeginUpdate();
                await _totalFluxDensityService.CreateAsync(allEnergyResults, project.Id.ToString(), project.CreatedBy);
                var totalFluxDensities = _totalFluxDensityService.GetAllByOid(project.Id.ToString());
                var maxTotalFluxDensities = totalFluxDensities.Result.Max(total => Math.Abs(total.Value));
                document.ReplaceAll("[Total]", $"Санитарно-защитная зона на высоте 2 м от уровня земли не устанавливается," +
                                               $" т.к. суммарный уровень плотности потока энергии от антенн в точке максимального" +
                                               $" значения на высоте 2 м от уровня земли составляет {maxTotalFluxDensities.ToString("F8")} мкВт/см²" +
                                               $" и не превышает значения Ппду 10 мкВт/см².", SearchOptions.WholeWord);
                CreateTableTotalFlux(document,tableFlow,totalFluxDensities.Result,positionTableTranslator+1);
                document.Unit = DevExpress.Office.DocumentUnit.Inch;
                var images = _repositoryWrapper.ProjectImageRepository
                    .GetAllByCondition(x => x.ProjectId == project.Id).ToList();
                string folderPath = Path.Combine(mainDir,"TemporaryFiles");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                if (images.Count != 0)
                {
                    foreach (var image in images)
                    {
                        var keywordsImage = document.FindAll("[Table]", SearchOptions.WholeWord);
                        var position = keywordsImage[0].Start;
                        if (image != images.First())
                        {
                            var sect = document.AppendSection();
                            position = sect.Range.Start;
                        }
                        byte[] imageBytes = image.Image;
                        
                        string filePathImage = Path.Combine(mainDir,"TemporaryFiles",$"{project.Id}{image.Id}.jpg");
                        File.WriteAllBytes(filePathImage, imageBytes);
                        Shape picture = document.Shapes.InsertPicture(position, DocumentImageSource.FromFile(filePathImage));
                        picture.Size = new SizeF(9f, 8f);
                        picture.HorizontalAlignment = ShapeHorizontalAlignment.Center;
                        picture.VerticalAlignment = ShapeVerticalAlignment.Center;
                        picture.Line.Color = Color.Black;
                    }
                }
                var keywordsLast = document.FindAll("[Table]",SearchOptions.WholeWord);
                document.Delete(keywordsLast[0]);
                wordProcessor.SaveDocument(filePathExport, DocumentFormat.OpenXml);
            }
            if (!File.Exists(filePathExport))
                return new BaseResponse<byte[]>(
                    Result: null,
                    Messages: new List<string>() {"Файл проекта для экспорта не найден"},
                    Success: false);
            var fileBytes = await File.ReadAllBytesAsync(filePathExport);
            return new BaseResponse<byte[]>(
                Result: fileBytes,
                Messages: new List<string>() {"Файл проекта для экспорта получен"},
                Success: true);
        }
        catch (Exception e)
        {
            return new BaseResponse<byte[]>(
                Result: null,
                Messages: new List<string>() {e.Message},
                Success: false);
        }
        finally
        {
            var project = await  _repositoryWrapper.ProjectRepository.GetByCondition(x =>
                x.Id.ToString() == oid);
            var antennae = _repositoryWrapper.ProjectAntennaRepository.GetAllByCondition(x => x.ProjectId == project.Id).ToList();
            var totalFluxDensities = _repositoryWrapper.TotalFluxDensityRepository
                .GetAllByCondition(x => x.ProjectId == project.Id).ToList();
            _repositoryWrapper.TotalFluxDensityRepository.Delete(totalFluxDensities);
            for (int i = 0; i < antennae.Count; i++)
            {
                var translators = _repositoryWrapper.AntennaTranslatorRepository.GetAllByCondition(x =>
                    x.ProjectAntennaId == antennae[i].Id).ToList();
                for (int j = 0; j < translators.Count; j++)
                {
                    var bioHazardRadii = _repositoryWrapper.BiohazardRadiusRepository
                        .GetAllByCondition(x => x.AntennaTranslatorId == translators[j].Id).ToList();
                    var summaryBiohazardRadii = _repositoryWrapper.SummaryBiohazardRadiusRepository
                        .GetAllByCondition(x => x.AntennaTranslatorId == translators[j].Id).ToList();
                    var energyResults = _repositoryWrapper.EnergyFlowRepository
                        .GetAllByCondition(x => x.AntennaTranslatorId == translators[j].Id).ToList();
                    _repositoryWrapper.BiohazardRadiusRepository.Delete(bioHazardRadii);
                    _repositoryWrapper.SummaryBiohazardRadiusRepository.Delete(summaryBiohazardRadii);
                    _repositoryWrapper.EnergyFlowRepository.Delete(energyResults);
                }
            }
            await _repositoryWrapper.Save();
        }
    }


    private void CreateDiagram(Document document,DocumentPosition position,decimal max,List<BiohazardRadius> biohazardRadii)
    {
        document.Unit = DevExpress.Office.DocumentUnit.Inch;
        var chartShape = document.Shapes.InsertChart(position,ChartType.ScatterSmooth);
        chartShape.Name = "Scatter Line chart";
        chartShape.Size = new SizeF(4.5f, 3.7f);
        chartShape.RelativeHorizontalPosition = ShapeRelativeHorizontalPosition.Column;
        chartShape.RelativeVerticalPosition = ShapeRelativeVerticalPosition.Line;
        chartShape.Offset = new PointF(0.95f, 0.65f);
        ChartObject chart = (ChartObject)chartShape.ChartFormat.Chart;
        Worksheet worksheet = (Worksheet)chartShape.ChartFormat.Worksheet;
        var maxRadius = biohazardRadii.Max(radiation => Math.Abs(radiation.BiohazardRadiusZ));
        max = (maxRadius > max) ? maxRadius : max;
        SpecifyChartData(worksheet,biohazardRadii);
        chart.SelectData(worksheet.Range.FromLTRB(0, 0, 1, 360));
        chart.Legend.Visible = false;
        chart.Title.Visible = true;
        chart.Title.Font.Size = 8;
        var text = "Ширина БОЗ в вертикальной плоскости на расстоянии Rx от \nантенны вдоль линии горизонта по направлению излучения";
        if (biohazardRadii.First().DirectionType == DirectionType.Horizontal.GetDescription())
            text = "Ширина БОЗ в горизонтальной плоскости на расстоянии Rx от \nантенны вдоль линии горизонта по направлению излучения";
        chart.Title.SetValue(text);
        Axis valueAxisX = chart.PrimaryAxes[1];
        valueAxisX.Scaling.AutoMax = false;
        valueAxisX.Scaling.Max = (int)max + 5;
        valueAxisX.Scaling.AutoMin = false;
        valueAxisX.Scaling.Min = ((int)max + 5) * -1;
        chart.Series[0].Outline.SetSolidFill(Color.FromArgb(0x00, 0x00, 0x00));
        chart.Series[0].Outline.Width = 1.2;
    }
    
    private void CreateDiagramSummary(Document document,DocumentPosition position,decimal max,List<SummaryBiohazardRadius> summaryBiohazardRadii)
    {
        document.Unit = DevExpress.Office.DocumentUnit.Inch;
        var chartShape = document.Shapes.InsertChart(position,ChartType.ScatterSmooth);
        chartShape.Name = "Scatter Line chart";
        chartShape.Size = new SizeF(4.5f, 3.7f);
        chartShape.RelativeHorizontalPosition = ShapeRelativeHorizontalPosition.Column;
        chartShape.RelativeVerticalPosition = ShapeRelativeVerticalPosition.Line;
        chartShape.Offset = new PointF(0.95f, 0.65f);
        ChartObject chart = (ChartObject)chartShape.ChartFormat.Chart;
        Worksheet worksheet = (Worksheet)chartShape.ChartFormat.Worksheet;
        var maxRadius = summaryBiohazardRadii.Max(radiation => Math.Abs(radiation.BiohazardRadiusZ));
        max = (maxRadius > max) ? maxRadius : max; 
        SpecifyChartDataSummary(worksheet,summaryBiohazardRadii);
        chart.SelectData(worksheet.Range.FromLTRB(0, 0, 1, 360));
        chart.Legend.Visible = false;
        chart.Title.Visible = true;
        chart.Title.Font.Size = 8;
        var text = "Ширина БОЗ в вертикальной плоскости на расстоянии Rx от \nантенны вдоль линии горизонта по направлению излучения";
        if (summaryBiohazardRadii.First().DirectionType == DirectionType.Horizontal)
            text = "Ширина БОЗ в горизонтальной плоскости на расстоянии Rx от \nантенны вдоль линии горизонта по направлению излучения";
        chart.Title.SetValue(text);
        Axis valueAxisX = chart.PrimaryAxes[1];
        valueAxisX.Scaling.AutoMax = false;
        valueAxisX.Scaling.Max = ((int)max + 5);
        valueAxisX.Scaling.AutoMin = false;
        valueAxisX.Scaling.Min = ((int)max + 5) * -1;
        chart.Series[0].Outline.SetSolidFill(Color.FromArgb(0x00, 0x00, 0x00));
        chart.Series[0].Outline.Width = 1.2;
    }
    
    private void SpecifyChartData(Worksheet sheet,List<BiohazardRadius> biohazard)
    {
        for (int i = 0; i < biohazard.Count; i++)
        {
            sheet[i, 0].Value = biohazard[i].BiohazardRadiusX;
            sheet[i, 1].Value = biohazard[i].BiohazardRadiusZ;
        }
    }
    
    private void SpecifyChartDataSummary(Worksheet sheet,List<SummaryBiohazardRadius> summaryBiohazardRadii)
    {
        for (int i = 0; i < summaryBiohazardRadii.Count; i++)
        {
            sheet[i, 0].Value = summaryBiohazardRadii[i].BiohazardRadiusX;
            sheet[i, 1].Value = summaryBiohazardRadii[i].BiohazardRadiusZ;
        }
    }
    
    private void CreateTableEnergyResult(Document document,Table table,List<EnergyResult> energyResults,int position)
    {
        var x = 2;
        for (int i = 0; i < energyResults.Count; i++)
            document.InsertText(table[position, x+i].Range.Start, energyResults[i].Value.ToString("F8"));
    }

    private void CreateTableTotalFlux(Document document,Table table,List<TotalFluxDensityDto> totalFluxDensities,int position)
    {
        var x = 2;
        for (int i = 0; i < totalFluxDensities.Count; i++)
            document.InsertText(table[position, x+i].Range.Start, totalFluxDensities[i].Value.ToString("F8"));
    }
    

    private void CreateTable360(Document document,Table table,List<BiohazardRadius> biohazardRadii,int maxRadiusDegree, int minDegreeZ,DirectionType type)
    {
        var maxRadius = biohazardRadii.Max(radiation => Math.Abs(radiation.MaximumBiohazardRadius));
        var radiationMaxRadius = biohazardRadii
            .First(radiation => Math.Abs(radiation.MaximumBiohazardRadius) == maxRadius);
        var minRadiusValueZ = biohazardRadii.Min(radiation => radiation.BiohazardRadiusZ);
        var radiationZ = biohazardRadii
            .First(radiation => radiation.BiohazardRadiusZ == minRadiusValueZ);
        if (type == DirectionType.Horizontal)
        {
            var maxRadiusValueZ = biohazardRadii.Max(radiation => Math.Abs(radiation.BiohazardRadiusZ) );
            radiationZ = biohazardRadii
                .First(radiation => Math.Abs(radiation.BiohazardRadiusZ) == maxRadiusValueZ);
        }
        var y = 1;
        for (int i = 0; i < biohazardRadii.Count; i++) 
        {
            var x = i % 10;
            if (x == 0 || biohazardRadii[i] == radiationMaxRadius || biohazardRadii[i] == radiationZ 
                || biohazardRadii[i].Degree == maxRadiusDegree || biohazardRadii[i].Degree == minDegreeZ) 
            {
                if (biohazardRadii[i].DirectionType == DirectionType.Horizontal.GetDescription())
                {
                    document.InsertText(table[y, 0].Range.Start, biohazardRadii[i].Degree.ToString());
                    document.InsertText(table[y, 1].Range.Start, biohazardRadii[i].Db.ToString("F3"));
                    document.InsertText(table[y, 2].Range.Start, biohazardRadii[i].DbRaz.ToString("F3"));
                    document.InsertText(table[y, 3].Range.Start, biohazardRadii[i].MaximumBiohazardRadius.ToString("F3"));
                    document.InsertText(table[y, 4].Range.Start, biohazardRadii[i].BiohazardRadiusZ.ToString("F3"));
                    document.InsertText(table[y, 5].Range.Start, biohazardRadii[i].BiohazardRadiusX.ToString("F3"));
                    y++;
                }
                if (biohazardRadii[i].DirectionType == DirectionType.Vertical.GetDescription())
                {
                    document.InsertText(table[y, 7].Range.Start, biohazardRadii[i].Degree.ToString());
                    document.InsertText(table[y, 8].Range.Start, biohazardRadii[i].Db.ToString("F3"));
                    document.InsertText(table[y, 9].Range.Start, biohazardRadii[i].DbRaz.ToString("F3"));
                    document.InsertText(table[y, 10].Range.Start, biohazardRadii[i].MaximumBiohazardRadius.ToString("F3"));
                    document.InsertText(table[y, 11].Range.Start, biohazardRadii[i].BiohazardRadiusZ.ToString("F3"));
                    document.InsertText(table[y, 12].Range.Start, biohazardRadii[i].BiohazardRadiusX.ToString("F3"));
                    y++;
                }
            }
        }
    }
    
    
    private void CreateTableMaximum360(Document document,Table table,List<SummaryBiohazardRadius> summaryBiohazardRadii,int maxRadiusDegree, int minDegreeZ,DirectionType type)
    {
        var maxRadius = summaryBiohazardRadii.Max(radiation => Math.Abs(radiation.MaximumBiohazardRadius));
        var radiationMaxRadius = summaryBiohazardRadii
            .First(radiation => Math.Abs(radiation.MaximumBiohazardRadius) == maxRadius);
        var minRadiusValueZ = summaryBiohazardRadii.Min(radiation => radiation.BiohazardRadiusZ);
        var radiationZ = summaryBiohazardRadii
            .First(radiation => radiation.BiohazardRadiusZ == minRadiusValueZ);
        if (type == DirectionType.Horizontal)
        {
            var maxRadiusValueZ = summaryBiohazardRadii.Max(radiation => Math.Abs(radiation.BiohazardRadiusZ) );
            radiationZ = summaryBiohazardRadii
                .First(radiation => Math.Abs(radiation.BiohazardRadiusZ) == maxRadiusValueZ);
        }
        var y = 1;
        for (int i = 0; i < summaryBiohazardRadii.Count; i++) 
        {
            var x = i % 10;
            if (x == 0 || summaryBiohazardRadii[i] == radiationMaxRadius || summaryBiohazardRadii[i] == radiationZ 
                || summaryBiohazardRadii[i].Degree == maxRadiusDegree || summaryBiohazardRadii[i].Degree == minDegreeZ) 
            {
                if (summaryBiohazardRadii[i].DirectionType == DirectionType.Horizontal)
                {
                    document.InsertText(table[y, 0].Range.Start, summaryBiohazardRadii[i].Degree.ToString());
                    document.InsertText(table[y, 1].Range.Start, summaryBiohazardRadii[i].MaximumBiohazardRadius.ToString("F3"));
                    document.InsertText(table[y, 2].Range.Start, summaryBiohazardRadii[i].BiohazardRadiusZ.ToString("F3"));
                    document.InsertText(table[y, 3].Range.Start, summaryBiohazardRadii[i].BiohazardRadiusX.ToString("F3"));
                    y++;
                }
                if (summaryBiohazardRadii[i].DirectionType == DirectionType.Vertical)
                {
                    document.InsertText(table[y, 5].Range.Start, summaryBiohazardRadii[i].Degree.ToString());
                    document.InsertText(table[y, 6].Range.Start, summaryBiohazardRadii[i].MaximumBiohazardRadius.ToString("F3"));
                    document.InsertText(table[y, 7].Range.Start, summaryBiohazardRadii[i].BiohazardRadiusZ.ToString("F3"));
                    document.InsertText(table[y, 8].Range.Start, summaryBiohazardRadii[i].BiohazardRadiusX.ToString("F3"));
                    y++;
                }
            }
        }
    }

    private int CheckCountTable(List<BiohazardRadius> biohazardRadii, int maxRadiusDegree, int degreeZ,DirectionType type)
    {
        var y = 0;
        var maxRadius = biohazardRadii.Max(radiation => Math.Abs(radiation.MaximumBiohazardRadius));
        var radiationMaxRadius = biohazardRadii
            .First(radiation => Math.Abs(radiation.MaximumBiohazardRadius) == maxRadius);
        var minRadiusValueZ = biohazardRadii.Min(radiation => radiation.BiohazardRadiusZ);
        var radiationZ = biohazardRadii
            .First(radiation => radiation.BiohazardRadiusZ == minRadiusValueZ);
        if (type == DirectionType.Horizontal)
        {
            var maxRadiusValueZ = biohazardRadii.Max(radiation => Math.Abs(radiation.BiohazardRadiusZ) );
            radiationZ = biohazardRadii
                .First(radiation => Math.Abs(radiation.BiohazardRadiusZ) == maxRadiusValueZ);
        }
        for (int i = 0; i < biohazardRadii.Count; i++) 
        {
            var x = i % 10;
            if (x == 0 || biohazardRadii[i] == radiationMaxRadius || biohazardRadii[i] == radiationZ
                || biohazardRadii[i].Degree == maxRadiusDegree || biohazardRadii[i].Degree == degreeZ)
                y++;
        }
        return y;
    }
    
    private int CheckMaxCountTable(List<SummaryBiohazardRadius> summaryBiohazardRadii, int maxRadiusDegree, int degreeZ,DirectionType type)
    {
        var y = 0;
        var maxRadius = summaryBiohazardRadii.Max(radiation => Math.Abs(radiation.MaximumBiohazardRadius));
        var radiationMaxRadius = summaryBiohazardRadii
            .First(radiation => Math.Abs(radiation.MaximumBiohazardRadius) == maxRadius);
        var minRadiusValueZ = summaryBiohazardRadii.Min(radiation => radiation.BiohazardRadiusZ);
        var radiationZ = summaryBiohazardRadii
            .First(radiation => radiation.BiohazardRadiusZ == minRadiusValueZ);
        if (type == DirectionType.Horizontal)
        {
            var maxRadiusValueZ = summaryBiohazardRadii.Max(radiation => Math.Abs(radiation.BiohazardRadiusZ) );
            radiationZ = summaryBiohazardRadii
                .First(radiation => Math.Abs(radiation.BiohazardRadiusZ) == maxRadiusValueZ);
        }
        for (int i = 0; i < summaryBiohazardRadii.Count; i++) 
        {
            var x = i % 10;
            if (x == 0 || summaryBiohazardRadii[i] == radiationMaxRadius || summaryBiohazardRadii[i] == radiationZ
                || summaryBiohazardRadii[i].Degree == maxRadiusDegree || summaryBiohazardRadii[i].Degree == degreeZ)
                y++;

        }
        return y;
    }
}