using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("DASHBOARD")]
public class HomeController : Controller
{
    private readonly IDashboardService _service;

    public HomeController(IDashboardService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";

        var summary = await _service.GetSummaryAsync();
        var topProducts = await _service.GetTopStockProductsAsync();
        var activities = await _service.GetRecentActivitiesAsync();
        var distribution = await _service.GetStockByTypeAsync();
        var criticalStock = await _service.GetCriticalStockProductsAsync();

        ViewBag.TopProducts = topProducts.Value;
        ViewBag.Activities = activities.Value;
        ViewBag.Distribution = distribution.Value;
        ViewBag.CriticalStock = criticalStock.Value;

        return View(summary.Value);
    }

    [AllowAnonymous]
    public IActionResult ShowStatusCode(int code)
    {
        ViewData["Title"] = code == 404 ? "Sayfa Bulunamadý" : "Bir Hata Oluþtu";
        ViewData["StatusCode"] = code;
        return View("StatusCode");
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        ViewData["Title"] = "Bir Hata Oluþtu";
        ViewData["StatusCode"] = 500;
        return View("StatusCode");
    }
}