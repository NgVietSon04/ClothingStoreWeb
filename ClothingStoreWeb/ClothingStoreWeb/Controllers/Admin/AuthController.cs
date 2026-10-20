using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>Đăng nhập / đăng xuất khu vực Admin (/Admin/Auth/Login).</summary>
[AllowAnonymous]
[Route("Admin/Auth")]
public class AuthController : Controller
{
    private readonly ClothingStoreDbContext _db;

    public AuthController(ClothingStoreDbContext db) => _db = db;

    [HttpGet("Login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        var current = await HttpContext.AuthenticateAsync(AdminAuth.Scheme);
        if (current.Succeeded) return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost("Login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        var username = vm.Username!.Trim();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);

        // So sánh mật khẩu phân biệt hoa/thường (collation SQL Server mặc định không phân biệt)
        if (user == null || !string.Equals(user.Password, vm.Password, StringComparison.Ordinal))
        {
            ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu.");
            return View(vm);
        }

        if (user.UserRole != 1)
        {
            ModelState.AddModelError("", "Tài khoản này không có quyền truy cập trang quản trị.");
            return View(vm);
        }

        var emp = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Username == user.Username);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, AdminAuth.Role),
            new("FullName", emp?.FullName ?? user.Username)
        };
        if (emp != null) claims.Add(new Claim("EmployeeId", emp.EmployeeId));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AdminAuth.Scheme));
        await HttpContext.SignInAsync(AdminAuth.Scheme, principal,
            new AuthenticationProperties { IsPersistent = vm.RememberMe });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("Logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AdminAuth.Scheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("Denied")]
    public IActionResult Denied() => View();
}
