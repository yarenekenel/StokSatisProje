using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakip.Core.Dto;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("PROJE_MALIYET")]
public class ReportsController : Controller
{
    private readonly IReportService _service;
    private readonly IProjectService _projectService;
    private readonly IProductService _productService;
    private readonly ICompanyService _companyService;

    public ReportsController(
        IReportService service,
        IProjectService projectService,
        IProductService productService,
        ICompanyService companyService)
    {
        _service = service;
        _projectService = projectService;
        _productService = productService;
        _companyService = companyService;
    }

    public async Task<IActionResult> StockBalance(int? productId, int? companyId, bool belowCriticalOnly = false)
    {
        ViewData["Title"] = "Stok Bakiye Raporu";
        ViewData["BelowCriticalOnly"] = belowCriticalOnly;
        ViewData["ProductId"] = productId;
        ViewData["CompanyId"] = companyId;

        var result = await _service.GetStockBalanceAsync(productId, companyId, belowCriticalOnly);

        var products = await _productService.GetLookupAsync();
        var companies = await _companyService.GetAllAsync(isActive: true);
        ViewBag.Products = products.Value;
        ViewBag.Companies = companies.Value;

        return View(result.Value);
    }

    public async Task<IActionResult> ExportStockBalance(int? productId, int? companyId, bool belowCriticalOnly = false)
    {
        var result = await _service.GetStockBalanceAsync(productId, companyId, belowCriticalOnly);
        var items = result.Value ?? Array.Empty<StockBalanceDto>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Stok Bakiye");

        sheet.Cell(1, 1).Value = "Ürün Kodu";
        sheet.Cell(1, 2).Value = "Ürün Adı";
        sheet.Cell(1, 3).Value = "Birim";
        sheet.Cell(1, 4).Value = "Toplam Giriş";
        sheet.Cell(1, 5).Value = "Toplam Çıkış";
        sheet.Cell(1, 6).Value = "Kalan Stok";
        sheet.Cell(1, 7).Value = "Kritik Seviye";
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.ProductCode;
            sheet.Cell(row, 2).Value = item.ProductName;
            sheet.Cell(row, 3).Value = item.Unit;
            sheet.Cell(row, 4).Value = item.TotalIn;
            sheet.Cell(row, 5).Value = item.TotalOut;
            sheet.Cell(row, 6).Value = item.RemainingStock;
            sheet.Cell(row, 7).Value = item.CriticalStockLevel;
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"StokBakiye_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportStockBalancePdf(int? productId, int? companyId, bool belowCriticalOnly = false)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _service.GetStockBalanceAsync(productId, companyId, belowCriticalOnly);
        var items = result.Value ?? Array.Empty<StockBalanceDto>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.Header().Text("Stok Bakiye Raporu").FontSize(18).Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Ürün Kodu").Bold();
                        header.Cell().Text("Ürün Adı").Bold();
                        header.Cell().Text("Birim").Bold();
                        header.Cell().Text("Toplam Giriş").Bold();
                        header.Cell().Text("Toplam Çıkış").Bold();
                        header.Cell().Text("Kalan Stok").Bold();
                        header.Cell().Text("Kritik Seviye").Bold();
                    });

                    foreach (var item in items)
                    {
                        table.Cell().Text(item.ProductCode);
                        table.Cell().Text(item.ProductName);
                        table.Cell().Text(item.Unit);
                        table.Cell().Text(item.TotalIn.ToString("N2"));
                        table.Cell().Text(item.TotalOut.ToString("N2"));
                        table.Cell().Text(item.RemainingStock.ToString("N2"));
                        table.Cell().Text(item.CriticalStockLevel?.ToString("N2") ?? "-");
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
        return File(pdfBytes, "application/pdf", $"StokBakiye_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    public async Task<IActionResult> ProjectCost(int? projectId, int? companyId, int? productId, DateTime? dateFrom, DateTime? dateTo)
    {
        ViewData["Title"] = "Proje Maliyet Raporu";
        ViewData["ProjectId"] = projectId;
        ViewData["CompanyId"] = companyId;
        ViewData["ProductId"] = productId;
        ViewData["DateFrom"] = dateFrom;
        ViewData["DateTo"] = dateTo;

        var projects = await _projectService.GetListAsync(isActive: null);
        var companies = await _companyService.GetAllAsync(isActive: null);
        var products = await _productService.GetLookupAsync();
        ViewBag.Projects = projects.Value;
        ViewBag.Companies = companies.Value;
        ViewBag.Products = products.Value;

        var result = await _service.GetProjectCostReportAsync(projectId, companyId, productId, dateFrom, dateTo);
        return View(result.Value);
    }
    public async Task<IActionResult> ProjectCostDetail(int projectId)
    {
        ViewData["Title"] = "Proje Maliyet Detayı";

        var summary = await _service.GetProjectCostAsync(projectId);
        if (!summary.IsSuccess)
            return NotFound();

        var detail = await _service.GetProjectCostDetailAsync(projectId);

        ViewBag.Summary = summary.Value;
        ViewBag.Detail = detail.Value;

        return View();
    }
    public async Task<IActionResult> ExportProjectCost(int? projectId, int? companyId, int? productId, DateTime? dateFrom, DateTime? dateTo)
    {
        var result = await _service.GetProjectCostReportAsync(projectId, companyId, productId, dateFrom, dateTo);
        var items = result.Value ?? Array.Empty<ProjectCostDto>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Proje Maliyet");

        sheet.Cell(1, 1).Value = "Proje Kodu";
        sheet.Cell(1, 2).Value = "Firma";
        sheet.Cell(1, 3).Value = "Teklif Tutarı (TL)";
        sheet.Cell(1, 4).Value = "Sarf (TL)";
        sheet.Cell(1, 5).Value = "Tahmini Kâr (TL)";
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.ProjectCode;
            sheet.Cell(row, 2).Value = item.CompanyName;
            sheet.Cell(row, 3).Value = item.OfferAmountTry;
            sheet.Cell(row, 4).Value = item.TotalIssueCostTry;
            sheet.Cell(row, 5).Value = item.EstimatedProfitTry;
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ProjeMaliyet_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportProjectCostPdf(int? projectId, int? companyId, int? productId, DateTime? dateFrom, DateTime? dateTo)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _service.GetProjectCostReportAsync(projectId, companyId, productId, dateFrom, dateTo);
        var items = result.Value ?? Array.Empty<ProjectCostDto>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.Header().Text("Proje Maliyet Raporu").FontSize(18).Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Proje Kodu").Bold();
                        header.Cell().Text("Firma").Bold();
                        header.Cell().Text("Teklif (TL)").Bold();
                        header.Cell().Text("Sarf (TL)").Bold();
                        header.Cell().Text("Tahmini Kâr (TL)").Bold();
                    });

                    foreach (var item in items)
                    {
                        table.Cell().Text(item.ProjectCode);
                        table.Cell().Text(item.CompanyName);
                        table.Cell().Text(item.OfferAmountTry?.ToString("N2") ?? "-");
                        table.Cell().Text(item.TotalIssueCostTry.ToString("N2"));
                        table.Cell().Text(item.EstimatedProfitTry?.ToString("N2") ?? "-");
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
        return File(pdfBytes, "application/pdf", $"ProjeMaliyet_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}