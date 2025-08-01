using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories;

/// <summary>
/// Product-specific repository interface
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategory(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProducts(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProducts(int threshold = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsByPriceRange(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalInventoryValue(CancellationToken cancellationToken = default);
    Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}