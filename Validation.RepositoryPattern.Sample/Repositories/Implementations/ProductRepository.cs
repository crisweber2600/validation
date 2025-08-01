using Microsoft.EntityFrameworkCore;
using Validation.RepositoryPattern.Sample.Data;
using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories.Implementations;

/// <summary>
/// Product repository implementation with product-specific operations
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategory(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProducts(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProducts(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.Quantity <= threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByPriceRange(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalInventoryValue(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .SumAsync(p => p.Price * p.Quantity, cancellationToken);
    }

    public async Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }
}