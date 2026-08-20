using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("PROJE_MAKINA_ATAMA")]
public class ProjectMachineController : Controller
{
    private readonly IProjectMachineService _service;
    private readonly IProjectService _projectService;
    private readonly IMachineService _machineService;

    public ProjectMachineController(
        IProjectMachineService service,
        IProjectService projectService,
        IMachineService machineService)
    {
        _service = service;
        _projectService = projectService;
        _machineService = machineService;
    }

    public async Task<IActionResult> Index(int? projectId)
    {
        ViewData["Title"] = "Proje-Makina Atamaları";
        ViewData["CurrentProjectId"] = projectId;

        var result = await _service.GetListAsync(projectId);

        var projects = await _projectService.GetListAsync(isActive: true);
        var machines = await _machineService.GetAllAsync(isActive: true);
        ViewBag.Projects = projects.Value;
        ViewBag.Machines = machines.Value;

        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(int projectId, int machineId)
    {
        var result = await _service.AssignAsync(projectId, machineId);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Makina projeye atandı." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Unassign(int id)
    {
        var result = await _service.UnassignAsync(id);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Atama kaldırıldı." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export(int? projectId)
    {
        var result = await _service.GetListAsync(projectId);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.ProjectMachineListDto>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Proje-Makina Atamaları");
        sheet.Cell(1, 1).Value = "Proje";
        sheet.Cell(1, 2).Value = "Makina Kodu";
        sheet.Cell(1, 3).Value = "Makina Adı";
        sheet.Cell(1, 4).Value = "Atama Tarihi";
        sheet.Row(1).Style.Font.Bold = true;
        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.ProjectCode;
            sheet.Cell(row, 2).Value = item.MachineCode;
            sheet.Cell(row, 3).Value = item.MachineName;
            sheet.Cell(row, 4).Value = item.AssignedAt.ToString("dd.MM.yyyy HH:mm");
            row++;
        }
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ProjeMakinaAtamalari_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf(int? projectId)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var result = await _service.GetListAsync(projectId);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.ProjectMachineListDto>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.Header().Text("Proje-Makina Atamaları").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); });
                    table.Header(h =>
                    {
                        h.Cell().Text("Proje").Bold();
                        h.Cell().Text("Makina Kodu").Bold();
                        h.Cell().Text("Makina Adı").Bold();
                        h.Cell().Text("Atama Tarihi").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text(i.ProjectCode);
                        table.Cell().Text(i.MachineCode);
                        table.Cell().Text(i.MachineName);
                        table.Cell().Text(i.AssignedAt.ToString("dd.MM.yyyy"));
                    }
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Oluşturulma: "); x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")); });
            });
        });

        return File(document.GeneratePdf(), "application/pdf", $"ProjeMakinaAtamalari_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}