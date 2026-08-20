using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using StokTakip.Service.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("URUNLER")]
public class ProductController : Controller
{
    private readonly IProductService _service;
    private readonly IProductTypeService _productTypeService;
    private readonly IUnitService _unitService;
    private readonly IStockTypeService _stockTypeService;

    public ProductController(
        IProductService service,
        IProductTypeService productTypeService,
        IUnitService unitService,
        IStockTypeService stockTypeService)
    {
        _service = service;
        _productTypeService = productTypeService;
        _unitService = unitService;
        _stockTypeService = stockTypeService;
    }

    public async Task<IActionResult> Index(bool? isActive)
    {
        ViewData["Title"] = "Ürünler";
        ViewData["CurrentFilter"] = isActive;

        var result = await _service.GetListAsync(isActive);
        await LoadLookupsAsync();

        return View(result.Value);
    }
    public async Task<IActionResult> Export(bool? isActive)
    {
        var result = await _service.GetListAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.ProductListDto>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Ürünler");

        sheet.Cell(1, 1).Value = "Kod";
        sheet.Cell(1, 2).Value = "Ad";
        sheet.Cell(1, 3).Value = "Ürün Tipi";
        sheet.Cell(1, 4).Value = "Birim";
        sheet.Cell(1, 5).Value = "Stok Tipi";
        sheet.Cell(1, 6).Value = "Muhasebe Stok Kodu";
        sheet.Cell(1, 7).Value = "Kritik Stok Seviyesi";
        sheet.Cell(1, 8).Value = "Durum";
        sheet.Row(1).Style.Font.Bold = true;



        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.Code;
            sheet.Cell(row, 2).Value = item.Name;
            sheet.Cell(row, 3).Value = item.ProductTypeName;
            sheet.Cell(row, 4).Value = item.UnitName;
            sheet.Cell(row, 5).Value = item.StockTypeName;
            sheet.Cell(row, 6).Value = item.AccountingStockCode;
            sheet.Cell(row, 7).Value = item.CriticalStockLevel;
            sheet.Cell(row, 8).Value = item.IsActive ? "Aktif" : "Pasif";
            row++;
        }



        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Urunler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf(bool? isActive)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _service.GetListAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.ProductListDto>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.Header().Text("Ürünler").FontSize(18).Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Kod").Bold();
                        h.Cell().Text("Ad").Bold();
                        h.Cell().Text("Ürün Tipi").Bold();
                        h.Cell().Text("Birim").Bold();
                        h.Cell().Text("Stok Tipi").Bold();
                        h.Cell().Text("Durum").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text(i.Code);
                        table.Cell().Text(i.Name);
                        table.Cell().Text(i.ProductTypeName);
                        table.Cell().Text(i.UnitName);
                        table.Cell().Text(i.StockTypeName);
                        table.Cell().Text(i.IsActive ? "Aktif" : "Pasif");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Oluşturulma: ");
                    x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Urunler_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
    [HttpGet]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound();

        var product = result.Value!;
        return Json(new
        {
            product.Id,
            product.Name,
            product.ProductTypeId,
            product.UnitId,
            product.StockTypeId,
            product.AccountingStockCode,
            product.CriticalStockLevel,
            product.Description
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string name, int productTypeId, int unitId, int stockTypeId,
        string? accountingStockCode, decimal? criticalStockLevel, string? description)
    {
        var result = await _service.CreateAsync(name, productTypeId, unitId, stockTypeId, accountingStockCode, criticalStockLevel, description);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Ürün eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        int id, string name, int productTypeId, int unitId, int stockTypeId,
        string? accountingStockCode, decimal? criticalStockLevel, string? description)
    {
        var result = await _service.UpdateAsync(id, name, productTypeId, unitId, stockTypeId, accountingStockCode, criticalStockLevel, description);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Ürün güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool isActive)
    {
        var result = await _service.SetActiveAsync(id, isActive);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? (isActive ? "Ürün aktif hale getirildi." : "Ürün pasif hale getirildi.") : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupsAsync()
    {
        var productTypes = await _productTypeService.GetAllAsync();
        var units = await _unitService.GetAllAsync();
        var stockTypes = await _stockTypeService.GetAllAsync();

        ViewBag.ProductTypes = productTypes.Value;
        ViewBag.Units = units.Value;
        ViewBag.StockTypes = stockTypes.Value;
    }
}