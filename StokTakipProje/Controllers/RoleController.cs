using Microsoft.AspNetCore.Mvc;
using StokTakip.Service.Interface;
using StokTakipProje.Authorization;

namespace StokTakipProje.Controllers;

[RequirePermission("ROL_YONETIMI")]
public class RoleController : Controller
{
    private readonly IRoleService _service;

    public RoleController(IRoleService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Rol Yönetimi";

        var roles = await _service.GetAllAsync();
        var permissions = await _service.GetAllPermissionsAsync();
        var matrix = await _service.GetRolePermissionMatrixAsync();

        ViewBag.Permissions = permissions.Value;
        ViewBag.Matrix = matrix.Value;

        return View(roles.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetPermissions(int roleId)
    {
        var result = await _service.GetPermissionIdsForRoleAsync(roleId);
        return Json(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, int[] permissionIds)
    {
        var result = await _service.CreateAsync(name, permissionIds ?? Array.Empty<int>());
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Rol eklendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string name, int[] permissionIds)
    {
        var result = await _service.UpdateAsync(id, name, permissionIds ?? Array.Empty<int>());
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Rol güncellendi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"]
            = result.IsSuccess ? "Rol silindi." : result.Error!.Message;
        return RedirectToAction(nameof(Index));
    }
}