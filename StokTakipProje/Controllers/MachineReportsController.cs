using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StokTakip.Core.Dto;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("MAKINA_RAPORLARI")]
public class MachineReportsController : Controller
{
    private readonly IMachineReportService _service;

    public MachineReportsController(IMachineReportService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(string tab = "consumption")
    {
        ViewData["Title"] = "Makina Raporları";
        ViewData["ActiveTab"] = tab;

        ViewBag.Consumption = (await _service.GetConsumptionAsync()).Value;
        ViewBag.Cost = (await _service.GetCostAsync()).Value;
        ViewBag.Usage = (await _service.GetUsageSummaryAsync()).Value;
        ViewBag.Distribution = (await _service.GetDistributionAsync()).Value;

        return View();
    }

    public async Task<IActionResult> Export(string tab)
    {
        using var workbook = new XLWorkbook();

        switch (tab)
        {
            case "consumption":
                {
                    var items = (await _service.GetConsumptionAsync()).Value ?? Array.Empty<MachineConsumptionReportDto>();
                    var sheet = workbook.Worksheets.Add("Makina Sarfiyat");
                    sheet.Cell(1, 1).Value = "Makina Kodu"; sheet.Cell(1, 2).Value = "Makina Adı";
                    sheet.Cell(1, 3).Value = "Ürün Kodu"; sheet.Cell(1, 4).Value = "Ürün Adı"; sheet.Cell(1, 5).Value = "Miktar";
                    sheet.Row(1).Style.Font.Bold = true;
                    var row = 2;
                    foreach (var i in items)
                    {
                        sheet.Cell(row, 1).Value = i.MachineCode; sheet.Cell(row, 2).Value = i.MachineName;
                        sheet.Cell(row, 3).Value = i.ProductCode; sheet.Cell(row, 4).Value = i.ProductName; sheet.Cell(row, 5).Value = i.Quantity;
                        row++;
                    }
                    sheet.Columns().AdjustToContents();
                    break;
                }
            case "cost":
                {
                    var items = (await _service.GetCostAsync()).Value ?? Array.Empty<MachineCostReportDto>();
                    var sheet = workbook.Worksheets.Add("Makina Maliyet");
                    sheet.Cell(1, 1).Value = "Makina Kodu"; sheet.Cell(1, 2).Value = "Makina Adı";
                    sheet.Cell(1, 3).Value = "Proje Kodu"; sheet.Cell(1, 4).Value = "Toplam Maliyet (TL)";
                    sheet.Row(1).Style.Font.Bold = true;
                    var row = 2;
                    foreach (var i in items)
                    {
                        sheet.Cell(row, 1).Value = i.MachineCode; sheet.Cell(row, 2).Value = i.MachineName;
                        sheet.Cell(row, 3).Value = i.ProjectCode; sheet.Cell(row, 4).Value = i.TotalCostTry;
                        row++;
                    }
                    sheet.Columns().AdjustToContents();
                    break;
                }
            case "usage":
                {
                    var items = (await _service.GetUsageSummaryAsync()).Value ?? Array.Empty<MachineUsageSummaryReportDto>();
                    var sheet = workbook.Worksheets.Add("Makina Kullanım Özeti");
                    sheet.Cell(1, 1).Value = "Makina Kodu"; sheet.Cell(1, 2).Value = "Makina Adı";
                    sheet.Cell(1, 3).Value = "Proje Sayısı"; sheet.Cell(1, 4).Value = "Toplam Miktar"; sheet.Cell(1, 5).Value = "Toplam Maliyet (TL)";
                    sheet.Row(1).Style.Font.Bold = true;
                    var row = 2;
                    foreach (var i in items)
                    {
                        sheet.Cell(row, 1).Value = i.MachineCode; sheet.Cell(row, 2).Value = i.MachineName;
                        sheet.Cell(row, 3).Value = i.ProjectCount; sheet.Cell(row, 4).Value = i.TotalQuantity; sheet.Cell(row, 5).Value = i.TotalCostTry;
                        row++;
                    }
                    sheet.Columns().AdjustToContents();
                    break;
                }
            case "distribution":
                {
                    var items = (await _service.GetDistributionAsync()).Value ?? Array.Empty<ProjectMachineDistributionReportDto>();
                    var sheet = workbook.Worksheets.Add("Proje-Makina Dağılımı");
                    sheet.Cell(1, 1).Value = "Proje Kodu"; sheet.Cell(1, 2).Value = "Makina Kodu";
                    sheet.Cell(1, 3).Value = "Makina Adı"; sheet.Cell(1, 4).Value = "Atama Tarihi";
                    sheet.Row(1).Style.Font.Bold = true;
                    var row = 2;
                    foreach (var i in items)
                    {
                        sheet.Cell(row, 1).Value = i.ProjectCode; sheet.Cell(row, 2).Value = i.MachineCode;
                        sheet.Cell(row, 3).Value = i.MachineName; sheet.Cell(row, 4).Value = i.AssignedAt.ToString("dd.MM.yyyy");
                        row++;
                    }
                    sheet.Columns().AdjustToContents();
                    break;
                }
            default:
                return BadRequest();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MakinaRaporu_{tab}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportPdf(string tab)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        string title;
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);

                if (tab == "cost")
                {
                    title = "Makina Maliyet Raporu";
                    var items = _service.GetCostAsync().Result.Value ?? Array.Empty<MachineCostReportDto>();
                    page.Header().Text(title).FontSize(18).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); });
                        table.Header(h =>
                        {
                            h.Cell().Text("Makina Kodu").Bold();
                            h.Cell().Text("Makina Adı").Bold();
                            h.Cell().Text("Proje Kodu").Bold();
                            h.Cell().Text("Toplam Maliyet (TL)").Bold();
                        });
                        foreach (var i in items)
                        {
                            table.Cell().Text(i.MachineCode);
                            table.Cell().Text(i.MachineName);
                            table.Cell().Text(i.ProjectCode);
                            table.Cell().Text(i.TotalCostTry.ToString("N2"));
                        }
                    });
                }
                else if (tab == "usage")
                {
                    title = "Makina Kullanım Özeti Raporu";
                    var items = _service.GetUsageSummaryAsync().Result.Value ?? Array.Empty<MachineUsageSummaryReportDto>();
                    page.Header().Text(title).FontSize(18).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                        table.Header(h =>
                        {
                            h.Cell().Text("Makina Kodu").Bold();
                            h.Cell().Text("Makina Adı").Bold();
                            h.Cell().Text("Proje Sayısı").Bold();
                            h.Cell().Text("Toplam Miktar").Bold();
                            h.Cell().Text("Toplam Maliyet (TL)").Bold();
                        });
                        foreach (var i in items)
                        {
                            table.Cell().Text(i.MachineCode);
                            table.Cell().Text(i.MachineName);
                            table.Cell().Text(i.ProjectCount.ToString());
                            table.Cell().Text(i.TotalQuantity.ToString("N2"));
                            table.Cell().Text(i.TotalCostTry.ToString("N2"));
                        }
                    });
                }
                else if (tab == "distribution")
                {
                    title = "Proje-Makina Dağılımı Raporu";
                    var items = _service.GetDistributionAsync().Result.Value ?? Array.Empty<ProjectMachineDistributionReportDto>();
                    page.Header().Text(title).FontSize(18).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); });
                        table.Header(h =>
                        {
                            h.Cell().Text("Proje Kodu").Bold();
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
                }
                else
                {
                    title = "Makina Sarfiyat Raporu";
                    var items = _service.GetConsumptionAsync().Result.Value ?? Array.Empty<MachineConsumptionReportDto>();
                    page.Header().Text(title).FontSize(18).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(); });
                        table.Header(h =>
                        {
                            h.Cell().Text("Makina Kodu").Bold();
                            h.Cell().Text("Makina Adı").Bold();
                            h.Cell().Text("Ürün Kodu").Bold();
                            h.Cell().Text("Ürün Adı").Bold();
                            h.Cell().Text("Miktar").Bold();
                        });
                        foreach (var i in items)
                        {
                            table.Cell().Text(i.MachineCode);
                            table.Cell().Text(i.MachineName);
                            table.Cell().Text(i.ProductCode);
                            table.Cell().Text(i.ProductName);
                            table.Cell().Text(i.Quantity.ToString("N2"));
                        }
                    });
                }

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Oluşturulma: ");
                    x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"MakinaRaporu_{tab}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}