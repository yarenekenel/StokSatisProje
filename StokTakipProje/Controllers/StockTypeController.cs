using Microsoft.AspNetCore.Mvc;
using StokTakip.Service.Interface;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakipProje.Authorization;


namespace StokTakipProje.Controllers;

[RequirePermission("STOK_TIPLERI")]
public class StockTypeController : Controller
{
    private readonly IStockTypeService _service;

    public StockTypeController(IStockTypeService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Stok Tipleri";
        var result = await _service.GetAllAsync();
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name)
    {
        var result = await _service.CreateAsync(name);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Stok tipi eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string name)
    {
        var result = await _service.UpdateAsync(id, name);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Stok tipi güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Stok tipi silindi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Export()
    {
        var result = await _service.GetAllAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.StockType>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Stok Tipleri");
        sheet.Cell(1, 1).Value = "Ad";
        sheet.Row(1).Style.Font.Bold = true;
        var row = 2;
        foreach (var item in items) { sheet.Cell(row, 1).Value = item.Name; row++; }
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"StokTipleri_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var result = await _service.GetAllAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.StockType>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.Header().Text("Stok Tipleri").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c => c.RelativeColumn());
                    table.Header(h => h.Cell().Text("Ad").Bold());
                    foreach (var i in items) table.Cell().Text(i.Name);
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Oluşturulma: "); x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")); });
            });
        });

        return File(document.GeneratePdf(), "application/pdf", $"StokTipleri_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}