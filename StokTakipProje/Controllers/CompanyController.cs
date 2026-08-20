using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("FIRMALAR")]
public class CompanyController : Controller
{
    private readonly ICompanyService _service;

    public CompanyController(ICompanyService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(bool? isActive)
    {
        ViewData["Title"] = "Firmalar";
        ViewData["CurrentFilter"] = isActive;
        var result = await _service.GetAllAsync(isActive);
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string companyName, string? taxNumber, string? phone, string? email, string? address)
    {
        var result = await _service.CreateAsync(companyName, taxNumber, phone, email, address);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Firma eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string companyName, string? taxNumber, string? phone, string? email, string? address)
    {
        var result = await _service.UpdateAsync(id, companyName, taxNumber, phone, email, address);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Firma güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool isActive)
    {
        var result = await _service.SetActiveAsync(id, isActive);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? (isActive ? "Firma aktif hale getirildi." : "Firma pasif hale getirildi.") : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export(bool? isActive)
    {
        var result = await _service.GetAllAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.Company>();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Firmalar");

        sheet.Cell(1, 1).Value = "Firma Adı";
        sheet.Cell(1, 2).Value = "Vergi No";
        sheet.Cell(1, 3).Value = "Telefon";
        sheet.Cell(1, 4).Value = "Email";
        sheet.Cell(1, 5).Value = "Adres";
        sheet.Cell(1, 6).Value = "Durum";
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.CompanyName;
            sheet.Cell(row, 2).Value = item.TaxNumber;
            sheet.Cell(row, 3).Value = item.Phone;
            sheet.Cell(row, 4).Value = item.Email;
            sheet.Cell(row, 5).Value = item.Address;
            sheet.Cell(row, 6).Value = item.IsActive ? "Aktif" : "Pasif";
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Firmalar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf(bool? isActive)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _service.GetAllAsync(isActive);
        var items = result.Value ?? Array.Empty<StokTakip.Core.Entity.Company>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.Header().Text("Firmalar").FontSize(18).Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Firma Adı").Bold();
                        h.Cell().Text("Vergi No").Bold();
                        h.Cell().Text("Telefon").Bold();
                        h.Cell().Text("Email").Bold();
                        h.Cell().Text("Durum").Bold();
                    });
                    foreach (var i in items)
                    {
                        table.Cell().Text(i.CompanyName);
                        table.Cell().Text(i.TaxNumber ?? "-");
                        table.Cell().Text(i.Phone ?? "-");
                        table.Cell().Text(i.Email ?? "-");
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
        return File(pdfBytes, "application/pdf", $"Firmalar_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}