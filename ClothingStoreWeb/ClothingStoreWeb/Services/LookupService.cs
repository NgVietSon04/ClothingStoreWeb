using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Services;

/// <summary>Cung cấp danh sách chọn (dropdown) cho các form.</summary>
public interface ILookupService
{
    Task<List<SelectListItem>> CountriesAsync();
    Task<List<SelectListItem>> BrandsAsync();
    Task<List<SelectListItem>> MaterialsAsync();
    Task<List<SelectListItem>> CategoriesAsync();
    Task<List<SelectListItem>> AudiencesAsync();
    Task<List<SelectListItem>> SizesAsync();
    Task<List<SelectListItem>> ColorsAsync();
    Task<List<SelectListItem>> SuppliersAsync();
    Task<List<SelectListItem>> UsersAsync();
    Task<List<SelectListItem>> CustomersAsync();
    Task<List<SelectListItem>> EmployeesAsync();
    Task<List<SelectListItem>> ProductsAsync();

    /// <summary>Danh sách biến thể kèm giá, giảm giá, tồn kho (dùng cho form đơn hàng / nhập kho).</summary>
    Task<List<VariantPickVm>> VariantPicksAsync();
}

public class LookupService : ILookupService
{
    private readonly ClothingStoreDbContext _db;

    public LookupService(ClothingStoreDbContext db) => _db = db;

    public Task<List<SelectListItem>> CountriesAsync() =>
        _db.Countries.AsNoTracking().OrderBy(x => x.CountryName)
            .Select(x => new SelectListItem { Value = x.CountryId, Text = x.CountryName ?? x.CountryId }).ToListAsync();

    public Task<List<SelectListItem>> BrandsAsync() =>
        _db.Brands.AsNoTracking().OrderBy(x => x.BrandName)
            .Select(x => new SelectListItem { Value = x.BrandId, Text = x.BrandName ?? x.BrandId }).ToListAsync();

    public Task<List<SelectListItem>> MaterialsAsync() =>
        _db.Materials.AsNoTracking().OrderBy(x => x.MaterialName)
            .Select(x => new SelectListItem { Value = x.MaterialId, Text = x.MaterialName ?? x.MaterialId }).ToListAsync();

    public Task<List<SelectListItem>> CategoriesAsync() =>
        _db.Categories.AsNoTracking().OrderBy(x => x.CategoryName)
            .Select(x => new SelectListItem { Value = x.CategoryId, Text = x.CategoryName ?? x.CategoryId }).ToListAsync();

    public Task<List<SelectListItem>> AudiencesAsync() =>
        _db.TargetAudiences.AsNoTracking().OrderBy(x => x.AudienceName)
            .Select(x => new SelectListItem { Value = x.AudienceId, Text = x.AudienceName ?? x.AudienceId }).ToListAsync();

    public Task<List<SelectListItem>> SizesAsync() =>
        _db.Sizes.AsNoTracking().OrderBy(x => x.SizeId)
            .Select(x => new SelectListItem { Value = x.SizeId, Text = x.SizeName ?? x.SizeId }).ToListAsync();

    public Task<List<SelectListItem>> ColorsAsync() =>
        _db.Colors.AsNoTracking().OrderBy(x => x.ColorName)
            .Select(x => new SelectListItem { Value = x.ColorId, Text = x.ColorName ?? x.ColorId }).ToListAsync();

    public Task<List<SelectListItem>> SuppliersAsync() =>
        _db.Suppliers.AsNoTracking().OrderBy(x => x.SupplierName)
            .Select(x => new SelectListItem { Value = x.SupplierId, Text = x.SupplierName }).ToListAsync();

    public Task<List<SelectListItem>> UsersAsync() =>
        _db.Users.AsNoTracking().OrderBy(x => x.Username)
            .Select(x => new SelectListItem { Value = x.Username, Text = x.Username }).ToListAsync();

    public Task<List<SelectListItem>> CustomersAsync() =>
        _db.Customers.AsNoTracking().OrderBy(x => x.CustomerId)
            .Select(x => new SelectListItem { Value = x.CustomerId, Text = x.CustomerId + " - " + (x.FullName ?? "") }).ToListAsync();

    public Task<List<SelectListItem>> EmployeesAsync() =>
        _db.Employees.AsNoTracking().OrderBy(x => x.EmployeeId)
            .Select(x => new SelectListItem { Value = x.EmployeeId, Text = x.EmployeeId + " - " + (x.FullName ?? "") }).ToListAsync();

    public Task<List<SelectListItem>> ProductsAsync() =>
        _db.Products.AsNoTracking().OrderBy(x => x.ProductId)
            .Select(x => new SelectListItem { Value = x.ProductId, Text = x.ProductId + " - " + (x.ProductName ?? "") }).ToListAsync();

    public async Task<List<VariantPickVm>> VariantPicksAsync()
    {
        var rows = await _db.ProductVariants.AsNoTracking()
            .OrderBy(v => v.VariantId)
            .Select(v => new
            {
                v.VariantId,
                Name = v.Product != null ? v.Product.ProductName : null,
                v.SizeId,
                v.ColorId,
                v.UnitPrice,
                v.Discount,
                v.StockQuantity
            }).ToListAsync();

        return rows.Select(v => new VariantPickVm
        {
            Id = v.VariantId,
            Text = $"{v.VariantId} · {v.Name} ({v.SizeId}/{v.ColorId}) · tồn {v.StockQuantity ?? 0}",
            Price = (decimal)(v.UnitPrice ?? 0),
            Discount = v.Discount ?? 0,
            Stock = v.StockQuantity ?? 0
        }).ToList();
    }
}
