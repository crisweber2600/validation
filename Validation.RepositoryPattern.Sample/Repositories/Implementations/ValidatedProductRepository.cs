using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.RepositoryPattern.Sample.Data;
using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories.Implementations;

/// <summary>
/// Validated Product repository that integrates validation with product-specific operations
/// </summary>
public class ValidatedProductRepository : ValidatedRepository<Product>, IProductRepository
{
    public ValidatedProductRepository(
        SampleDbContext context, 
        IEnhancedManualValidatorService validator,
        ILogger<ValidatedProductRepository> logger) : base(context, validator, logger)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategory(string category, CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.Category == category && p.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProducts(CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProducts(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.IsActive && p.Quantity <= threshold, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByPriceRange(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice, cancellationToken);
    }

    public async Task<decimal> GetTotalInventoryValue(CancellationToken cancellationToken = default)
    {
        var activeProducts = await GetActiveProducts(cancellationToken);
        return activeProducts.Sum(p => p.GetTotalValue());
    }

    public async Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }
}