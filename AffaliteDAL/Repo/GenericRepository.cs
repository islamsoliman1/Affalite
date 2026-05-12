using AffaliteDAL.Data;
using Microsoft.EntityFrameworkCore;

namespace AffaliteDAL.IRepo;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AffaliteDBContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AffaliteDBContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    // Synchronous methods
    public IEnumerable<T> GetAll()
    {
        return _dbSet.ToList();
    }

    public T? GetById(int id)
    {
        return _dbSet.Find(id);
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public IQueryable<T> GetAllQueryable()
    {
        return _dbSet.AsQueryable();
    }

    // Asynchronous methods
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
