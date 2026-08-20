using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("PROJELER")]
public class ProjectController : Controller
{
    private readonly IProjectService _service;
    private readonly ICompanyService _companyService;
    private readonly ICurrencyService _currencyService;

    public ProjectController(
        IProjectService service,
        ICompanyService companyService,
        ICurrencyService currencyService)
    {
        _service = service;
        _companyService = companyService;
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Index(bool? isActive)
    {
        ViewData["Title"] = "Projeler";
        ViewData["CurrentFilter"] = isActive;

        var result = await _service.GetListAsync(isActive);
        await LoadLookupsAsync();

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound();

        var project = result.Value!;
        return Json(new
        {
            project.Id,
            project.ProjectCode,
            project.CompanyId,
            project.OfferAmount,
            project.CurrencyId,
            project.ExchangeRate,
            StartDate = project.StartDate?.ToString("yyyy-MM-dd"),
            project.Description
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string projectCode, int companyId, decimal? offerAmount,
        int? currencyId, decimal? exchangeRate, DateTime? startDate, string? description)
    {
        var result = await _service.CreateAsync(projectCode, companyId, offerAmount, currencyId, exchangeRate, startDate, description);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Proje eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        int id, string projectCode, int companyId, decimal? offerAmount,
        int? currencyId, decimal? exchangeRate, DateTime? startDate, string? description)
    {
        var result = await _service.UpdateAsync(id, projectCode, companyId, offerAmount, currencyId, exchangeRate, startDate, description);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Proje güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool isActive)
    {
        var result = await _service.SetActiveAsync(id, isActive);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? (isActive ? "Proje aktif hale getirildi." : "Proje pasif hale getirildi.") : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Export(bool? isActive)
    {
        var result = await _service.GetListAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.ProjectListDto>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Projeler");

        sheet.Cell(1, 1).Value = "Proje Kodu";
        sheet.Cell(1, 2).Value = "Firma";
        sheet.Cell(1, 3).Value = "Teklif Tutarı";
        sheet.Cell(1, 4).Value = "Para Birimi";
        sheet.Cell(1, 5).Value = "Başlangıç Tarihi";
        sheet.Cell(1, 6).Value = "Durum";
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.ProjectCode;
            sheet.Cell(row, 2).Value = item.CompanyName;
            sheet.Cell(row, 3).Value = item.OfferAmount;
            sheet.Cell(row, 4).Value = item.CurrencyCode;
            sheet.Cell(row, 5).Value = item.StartDate?.ToString("dd.MM.yyyy");
            sheet.Cell(row, 6).Value = item.IsActive ? "Aktif" : "Pasif";
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Projeler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf(bool? isActive)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _service.GetListAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Dto.ProjectListDto>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.Header().Text("Projeler").FontSize(18).Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Proje Kodu").Bold();
                        h.Cell().Text("Firma").Bold();
                        h.Cell().Text("Teklif Tutarı").Bold();
                        h.Cell().Text("Para Birimi").Bold();
                        h.Cell().Text("Başlangıç").Bold();
                        h.Cell().Text("Durum").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text(i.ProjectCode);
                        table.Cell().Text(i.CompanyName);
                        table.Cell().Text(i.OfferAmount?.ToString("N2") ?? "-");
                        table.Cell().Text(i.CurrencyCode ?? "-");
                        table.Cell().Text(i.StartDate?.ToString("dd.MM.yyyy") ?? "-");
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
        return File(pdfBytes, "application/pdf", $"Projeler_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    private async Task LoadLookupsAsync()
    {
        var companies = await _companyService.GetAllAsync(isActive: true);
        var currencies = await _currencyService.GetAllAsync();

        ViewBag.Companies = companies.Value;
        ViewBag.Currencies = currencies.Value;
    }
}