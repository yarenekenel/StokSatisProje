using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StokTakip.Service.Interface;
using System.Security.Claims;

namespace StokTakipProje.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _service;
    private readonly IRoleService _roleService;

    public AccountController(IUserService service, IRoleService roleService)
    {
        _service = service;
        _roleService = roleService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var result = await _service.LoginAsync(username, password);

        if (!result.IsSuccess)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ErrorMessage"] = result.Error!.Message;
            return View();
        }

        var user = result.Value!;
        var permissionsResult = await _roleService.GetPermissionCodesForUserAsync(user.Id);
        var permissionCodes = (permissionsResult.Value ?? Array.Empty<string>()).ToList();

        if (permissionCodes.Count == 0)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ErrorMessage"] = "Hesabınıza herhangi bir yetki tanımlanmamış. Lütfen yöneticinizle iletişime geçin.";
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FullName ?? user.Username),
            new(ClaimTypes.Role, user.RoleName ?? string.Empty)
        };

        foreach (var code in permissionCodes)
        {
            claims.Add(new Claim("permission", code));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = false
        });

        if (permissionCodes.Contains("DASHBOARD"))
            return RedirectToAction("Index", "Home");

        if (permissionCodes.Contains("RAPORLAR"))
            return RedirectToAction("StockBalance", "Reports");

        if (permissionCodes.Contains("KARTLAR"))
            return RedirectToAction("Index", "Product");

        if (permissionCodes.Contains("STOK_HAREKETLERI"))
            return RedirectToAction("Index", "StockEntry");

        return RedirectToAction("Index", "User");
    }

    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}