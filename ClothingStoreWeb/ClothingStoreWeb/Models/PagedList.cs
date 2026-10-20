using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Models;

public interface IPaged
{
    int Page { get; }
    int TotalPages { get; }
    int TotalItems { get; }
}

public class PagedList<T> : IPaged
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int page, int pageSize = 10)
    {
        if (page < 1) page = 1;
        var total = await source.CountAsync();
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        if (page > pages) page = pages;
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T> { Items = items, Page = page, PageSize = pageSize, TotalItems = total };
    }
}
