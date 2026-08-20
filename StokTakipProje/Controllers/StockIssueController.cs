using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("STOK_CIKISI")]
public class StockIssueController : Controller
{
    private readonly IStockIssueService _service;
    private readonly IProjectService _projectService;
    private readonly IProjectMachineService _projectMachineService;
    private readonly IProductService _productService;

    public StockIssueController(
        IStockIssueService service,
        IProjectService projectService,
        IProjectMachineService projectMachineService,
        IProductService productService)
    {
        _service = service;
        _projectService = projectService;
        _projectMachineService = projectMachineService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Stok Çıkışı";

        var result = await _service.GetListAsync();

        var projects = await _projectService.GetListAsync(isActive: true);
        var products = await _productService.GetLookupAsync();
        ViewBag.Projects = projects.Value;
        ViewBag.Products = products.Value;

        return View(result.Value);
    }

    public async Task<IActionResult> Export()
    {
        var result = await _service.GetListAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.StockIssueListDto>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Stok Çıkışı");

        sheet.Cell(1, 1).Value = "Proje";
        sheet.Cell(1, 2).Value = "Makina Kodu";
        sheet.Cell(1, 3).Value = "Makina Adı";
        sheet.Cell(1, 4).Value = "Ürün Kodu";
        sheet.Cell(1, 5).Value = "Ürün Adı";
        sheet.Cell(1, 6).Value = "Miktar";
        sheet.Cell(1, 7).Value = "Birim Fiyat (TL)";
        sheet.Cell(1, 8).Value = "Tutar (TL)";
        sheet.Cell(1, 9).Value = "Tarih";
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.ProjectCode;
            sheet.Cell(row, 2).Value = item.MachineCode;
            sheet.Cell(row, 3).Value = item.MachineName;
            sheet.Cell(row, 4).Value = item.ProductCode;
            sheet.Cell(row, 5).Value = item.ProductName;
            sheet.Cell(row, 6).Value = item.Quantity;
            sheet.Cell(row, 7).Value = item.UnitPrice;
            sheet.Cell(row, 8).Value = item.AmountTry;
            sheet.Cell(row, 9).Value = item.Date.ToString("dd.MM.yyyy");
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"StokCikisi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
    public async Task<IActionResult> ExportPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _service.GetListAsync();
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.StockIssueListDto>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.Header().Text("Stok Çıkışı").FontSize(18).Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Proje").Bold();
                        h.Cell().Text("Makina").Bold();
                        h.Cell().Text("Ürün").Bold();
                        h.Cell().Text("Miktar").Bold();
                        h.Cell().Text("Tutar (TL)").Bold();
                        h.Cell().Text("Tarih").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text(i.ProjectCode);
                        table.Cell().Text($"{i.MachineCode} - {i.MachineName}");
                        table.Cell().Text($"{i.ProductCode} - {i.ProductName}");
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
        return File(pdfBytes, "application/pdf", $"StokCikisi_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> GetMachinesForProject(int projectId)
    {
        var result = await _projectMachineService.GetLookupByProjectIdAsync(projectId);
        if (!result.IsSuccess)
            return Json(Array.Empty<object>());

        return Json(result.Value!.Select(x => new { x.Id, text = $"{x.MachineCode} - {x.MachineName}" }));
    }

    [HttpGet]
    public async Task<IActionResult> GetLotsForProduct(int productId)
    {
        var result = await _service.GetOpenLotsLookupAsync();
        if (!result.IsSuccess)
            return Json(Array.Empty<object>());

        var lots = result.Value!.Where(x => x.ProductId == productId)
            .Select(x => new
            {
                x.StockEntryId,
                text = $"{x.GoodsReceiptNo ?? "-"} | {x.Date:dd.MM.yyyy} | Kalan: {x.RemainingQuantity:N2} {x.UnitName}"
            });

        return Json(lots);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLine(
        int projectMachineId, int productId, decimal quantity, DateTime date, string? description, long? stockEntryId)
    {
        var result = await _service.CreateLineAsync(projectMachineId, productId, quantity, date, description, stockEntryId);
        if (!result.IsSuccess)
            return Json(new { success = false, message = result.Error!.Message });

        return Json(new { success = true });
    }
}