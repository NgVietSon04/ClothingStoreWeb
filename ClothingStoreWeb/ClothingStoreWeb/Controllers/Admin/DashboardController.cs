using Microsoft.AspNetCore.Mvc;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>Trang tổng quan (/Admin).</summary>
[Route("Admin")]
public class DashboardController : AdminControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet(""), HttpGet("Dashboard")]
    public async Task<IActionResult> Index() => View(await _dashboard.GetAsync());
}
