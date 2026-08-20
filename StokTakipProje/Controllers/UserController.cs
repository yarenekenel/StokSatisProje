using Microsoft.AspNetCore.Mvc;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("KULLANICI_YONETIMI")]
public class UserController : Controller
{
    private readonly IUserService _service;
    private readonly IRoleService _roleService;

    public UserController(IUserService service, IRoleService roleService)
    {
        _service = service;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Kullanıcı Yönetimi";
        var result = await _service.GetAllAsync();
        var roles = await _roleService.GetAllAsync();
        ViewBag.Roles = roles.Value;
        return View(result.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(string username, string password, string? fullName, int? roleId)
    {
        var result = await _service.CreateAsync(username, password, fullName, roleId);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Kullanıcı eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string? fullName, string? newPassword, int? roleId)
    {
        var result = await _service.UpdateAsync(id, fullName, newPassword, roleId);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Kullanıcı güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool isActive)
    {
        var result = await _service.SetActiveAsync(id, isActive);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? (isActive ? "Kullanıcı aktif hale getirildi." : "Kullanıcı pasif hale getirildi.") : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Kullanıcı silindi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }
}