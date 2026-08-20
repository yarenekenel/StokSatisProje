using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using StokTakip.Service.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("STOK_GIRISI")]
public class StockEntryController : Controller
{
    private readonly IStockEntryService _service;
    private readonly IProductService _productService;
    private readonly ICompanyService _companyService;
    private readonly ICurrencyService _currencyService;

    public StockEntryController(
        IStockEntryService service,
        IProductService productService,
        ICompanyService companyService,
        ICurrencyService currencyService)
    {
        _service = service;
        _productService = productService;
        _companyService = companyService;
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Stok Girişi";

        var result = await _service.GetListAsync();
        await LoadLookupsAsync();

        return View(result.Value);
    }
    public async Task<IActionResult> Export()
    {
        var result = await _service.GetListAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.StockEntryListDto>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Stok Girişi");

        sheet.Cell(1, 1).Value = "Ürün Kodu";
        sheet.Cell(1, 2).Value = "Ürün Adı";
        sheet.Cell(1, 3).Value = "Firma";
        sheet.Cell(1, 4).Value = "Fatura No";
        sheet.Cell(1, 5).Value = "Mal Kabul No";
        sheet.Cell(1, 6).Value = "Miktar";
        sheet.Cell(1, 7).Value = "Birim Fiyat";
        sheet.Cell(1, 8).Value = "Para Birimi";
        sheet.Cell(1, 9).Value = "Tutar (TL)";
        sheet.Cell(1, 10).Value = "Tarih";
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.ProductCode;
            sheet.Cell(row, 2).Value = item.ProductName;
            sheet.Cell(row, 3).Value = item.CompanyName;
            sheet.Cell(row, 4).Value = item.InvoiceNo;
            sheet.Cell(row, 5).Value = item.GoodsReceiptNo;
            sheet.Cell(row, 6).Value = item.Quantity;
            sheet.Cell(row, 7).Value = item.UnitPrice;
            sheet.Cell(row, 8).Value = item.CurrencyCode;
            sheet.Cell(row, 9).Value = item.AmountTry;
            sheet.Cell(row, 10).Value = item.Date.ToString("dd.MM.yyyy");
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"StokGirisi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> GetDetail(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound();

        var entry = result.Value!;
        return Json(new
        {
            entry.Id,
            entry.ProductId,
            entry.CompanyId,
            entry.InvoiceNo,
            entry.GoodsReceiptNo,
            entry.ReceivedBy,
            entry.Quantity,
            entry.UnitPrice,
            entry.CurrencyId,
            entry.ExchangeRate,
            Date = entry.Date.ToString("yyyy-MM-dd"),
            entry.Description
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int productId, int? companyId, string? invoiceNo, string? goodsReceiptNo, string? receivedBy,
        decimal quantity, decimal unitPrice, int currencyId, decimal exchangeRate, DateTime date, string? description)
    {
        var result = await _service.CreateAsync(productId, companyId, invoiceNo, goodsReceiptNo, receivedBy, quantity, unitPrice, currencyId, exchangeRate, date, description);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Stok girişi eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        long id, int productId, int? companyId, string? invoiceNo, string? goodsReceiptNo, string? receivedBy,
        decimal quantity, decimal unitPrice, int currencyId, decimal exchangeRate, DateTime date, string? description)
    {
        var result = await _service.UpdateAsync(id, productId, companyId, invoiceNo, goodsReceiptNo, receivedBy, quantity, unitPrice, currencyId, exchangeRate, date, description);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Stok girişi güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Stok girişi silindi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> ExportPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _service.GetListAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.StockEntryListDto>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.Header().Text("Stok Girişi").FontSize(18).Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Ürün").Bold();
                        h.Cell().Text("Firma").Bold();
                        h.Cell().Text("Mal Kabul No").Bold();
                        h.Cell().Text("Miktar").Bold();
                        h.Cell().Text("Tutar (TL)").Bold();
                        h.Cell().Text("Tarih").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text($"{i.ProductCode} - {i.ProductName}");
                        table.Cell().Text(i.CompanyName ?? "-");
                        table.Cell().Text(i.GoodsReceiptNo ?? "-");
                        table.Cell().Text(i.Quantity.ToString("N2"));
                        table.Cell().Text(i.AmountTry.ToString("N2"));
                        table.Cell().Text(i.Date.ToString("dd.MM.yyyy"));
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
        return File(pdfBytes, "application/pdf", $"StokGirisi_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
    private async Task LoadLookupsAsync()
    {
        var products = await _productService.GetLookupAsync();
        var companies = await _companyService.GetAllAsync(isActive: true);
        var currencies = await _currencyService.GetAllAsync();

        ViewBag.Products = products.Value;
        ViewBag.Companies = companies.Value;
        ViewBag.Currencies = currencies.Value;
    }
}