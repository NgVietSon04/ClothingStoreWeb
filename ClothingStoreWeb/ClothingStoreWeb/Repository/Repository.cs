using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Repository;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly ClothingStoreDbContext _db;
    private readonly DbSet<T> _set;

    public GenericRepository(ClothingStoreDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public IQueryable<T> Table => _set;

    public async Task<T?> FindAsync(params object[] keys) => await _set.FindAsync(keys);

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public Task<int> SaveAsync() => _db.SaveChangesAsync();
}
