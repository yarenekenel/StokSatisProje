using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("URUN_TIPLERI")]
public class ProductTypeController : Controller
{
    private readonly IProductTypeService _service;

    public ProductTypeController(IProductTypeService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Ürün Tipleri";
        var result = await _service.GetAllAsync();
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, string codePrefix)
    {
        var result = await _service.CreateAsync(name, codePrefix);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Ürün tipi eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string name, string codePrefix)
    {
        var result = await _service.UpdateAsync(id, name, codePrefix);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Ürün tipi güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Ürün tipi silindi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export()
    {
        var result = await _service.GetAllAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.ProductType>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Ürün Tipleri");
        sheet.Cell(1, 1).Value = "Ad";
        sheet.Cell(1, 2).Value = "Kod Ön Eki";
        sheet.Row(1).Style.Font.Bold = true;
        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.Name;
            sheet.Cell(row, 2).Value = item.CodePrefix;
            row++;
        }
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"UrunTipleri_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var result = await _service.GetAllAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.ProductType>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.Header().Text("Ürün Tipleri").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); });
                    table.Header(h =>
                    {
                        h.Cell().Text("Ad").Bold();
                        h.Cell().Text("Kod Ön Eki").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text(i.Name);
                        table.Cell().Text(i.CodePrefix);
                    }
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Oluşturulma: "); x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")); });
            });
        });

        return File(document.GeneratePdf(), "application/pdf", $"UrunTipleri_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}