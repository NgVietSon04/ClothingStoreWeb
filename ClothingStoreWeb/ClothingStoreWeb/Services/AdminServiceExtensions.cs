using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Repository;

namespace ClothingStoreWeb.Services;

public static class AdminServiceExtensions
{
    /// <summary>
    /// Đăng ký toàn bộ dịch vụ cho khu vực Admin. Gọi 1 lần trong Program.cs:
    /// builder.Services.AddAdminModule(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddAdminModule(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("QLShop")
                               ?? config.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Thiếu chuỗi kết nối: thêm ConnectionStrings:QLShop vào appsettings.json");

        services.AddDbContext<ClothingStoreDbContext>(o => o.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // View của admin nằm ở Views/Admin/{Controller}/{View}.cshtml
        services.Configure<RazorViewEngineOptions>(o =>
        {
            o.ViewLocationFormats.Add("/Views/Admin/{1}/{0}.cshtml");
            o.ViewLocationFormats.Add("/Views/Admin/Shared/{0}.cshtml");
        });

        // Cookie đăng nhập riêng cho admin (không ảnh hưởng đăng nhập của khách hàng)
        services.AddAuthentication().AddCookie(AdminAuth.Scheme, o =>
        {
            o.LoginPath = AdminAuth.LoginPath;
            o.AccessDeniedPath = AdminAuth.DeniedPath;
            o.Cookie.Name = ".QLShop.Admin";
            o.Cookie.HttpOnly = true;
            o.ExpireTimeSpan = TimeSpan.FromHours(8);
            o.SlidingExpiration = true;
        });
        services.AddAuthorization();

        var imageBase = config["Admin:ImageBase"];
        if (!string.IsNullOrWhiteSpace(imageBase)) AdminUi.ImageBase = imageBase;

        return services;
    }
}
