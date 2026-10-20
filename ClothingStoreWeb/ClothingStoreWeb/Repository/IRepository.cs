namespace ClothingStoreWeb.Repository;

/// <summary>Repository chung cho mọi entity của khu vực Admin.</summary>
public interface IRepository<T> where T : class
{
    /// <summary>Truy vấn gốc (có tracking). Dùng .AsNoTracking() khi chỉ đọc.</summary>
    IQueryable<T> Table { get; }

    Task<T?> FindAsync(params object[] keys);
    Task AddAsync(T entity);
    void Remove(T entity);
    Task<int> SaveAsync();
}
