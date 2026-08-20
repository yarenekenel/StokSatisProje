using Microsoft.AspNetCore.Mvc;
using StokTakip.Service.Interface;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("MAKINALAR")]
public class MachineController : Controller
{
    private readonly IMachineService _service;
    private readonly IProjectService _projectService;

    public MachineController(IMachineService service, IProjectService projectService)
    {
        _service = service;
        _projectService = projectService;
    }

    public async Task<IActionResult> Index(bool? isActive)
    {
        ViewData["Title"] = "Makinalar";
        ViewData["CurrentFilter"] = isActive;

        var result = await _service.GetAllAsync(isActive);

        var projects = await _projectService.GetListAsync(isActive: true);
        ViewBag.Projects = projects.Value;

        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int projectId, string name)
    {
        var result = await _service.CreateForProjectAsync(projectId, name);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? $"Makina eklendi. Kod: {result.Value!.Code}" : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string name)
    {
        var result = await _service.UpdateAsync(id, name);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Makina güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool isActive)
    {
        var result = await _service.SetActiveAsync(id, isActive);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? (isActive ? "Makina aktif hale getirildi." : "Makina pasif hale getirildi.") : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export(bool? isActive)
    {
        var result = await _service.GetAllAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.Machine>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Makinalar");
        sheet.Cell(1, 1).Value = "Kod";
        sheet.Cell(1, 2).Value = "Ad";
        sheet.Cell(1, 3).Value = "Durum";
        sheet.Row(1).Style.Font.Bold = true;
        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.Code;
            sheet.Cell(row, 2).Value = item.Name;
            sheet.Cell(row, 3).Value = item.IsActive ? "Aktif" : "Pasif";
            row++;
        }
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Makinalar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf(bool? isActive)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var result = await _service.GetAllAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.Machine>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.Header().Text("Makinalar").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); });
                    table.Header(h =>
                    {
                        h.Cell().Text("Kod").Bold();
                        h.Cell().Text("Ad").Bold();
                        h.Cell().Text("Durum").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text(i.Code);
                        table.Cell().Text(i.Name);
                        table.Cell().Text(i.IsActive ? "Aktif" : "Pasif");
                    }
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Oluşturulma: "); x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")); });
            });
        });

        return File(document.GeneratePdf(), "application/pdf", $"Makinalar_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}