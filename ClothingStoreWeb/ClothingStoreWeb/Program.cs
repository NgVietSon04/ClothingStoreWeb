using ClothingStoreWeb.Models;
using ClothingStoreWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<RazorViewEngineOptions>(o =>
{
    o.ViewLocationFormats.Add("/Views/User/{1}/{0}.cshtml");
    o.ViewLocationFormats.Add("/Views/User/Shared/{0}.cshtml");
});

builder.Services.AddAdminModule(builder.Configuration);

// Đọc cấu hình ShopSettings từ appsettings.json
builder.Services.Configure<ShopSettings>(builder.Configuration.GetSection("ShopSettings"));

// 1. KẾT NỐI SQL SERVER
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClothingStoreDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. COOKIE AUTHENTICATION
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/LoginRegister";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// 3. DỮ LIỆU DÙNG CHUNG CHO LAYOUT
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILayoutDataService, LayoutDataService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 4. ĐỊNH TUYẾN AREA (QUAN TRỌNG ĐỂ KHÔNG BỊ LỖI 404 CHO ADMIN)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 5. ĐỊNH TUYẾN MẶC ĐỊNH CHO USER
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();