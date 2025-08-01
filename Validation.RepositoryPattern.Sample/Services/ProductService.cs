using Validation.RepositoryPattern.Sample.Models;
using Validation.RepositoryPattern.Sample.Repositories;

namespace Validation.RepositoryPattern.Sample.Services;

/// <summary>
/// Product service interface for business operations
/// </summary>
public interface IProductService
{
    // Basic operations
    Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Business operations
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default);
    Task<Product> UpdateProductPriceAsync(Guid id, decimal newPrice, CancellationToken cancellationToken = default);
    Task<Product> UpdateProductQuantityAsync(Guid id, int newQuantity, CancellationToken cancellationToken = default);
    Task<decimal> GetInventoryValueAsync(CancellationToken cancellationToken = default);
    Task<bool> IsProductNameAvailableAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Product service implementation using repository pattern with validation
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetActiveProducts(cancellationToken);
    }

    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        // Check if name is already taken
        var existingProduct = await _productRepository.GetByNameAsync(product.Name, cancellationToken);
        if (existingProduct != null)
        {
            throw new InvalidOperationException($"A product with name '{product.Name}' already exists.");
        }
        
        // Repository will handle validation
        var addedProduct = await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        
        return addedProduct;
    }

    public async Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        var existingProduct = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        if (existingProduct == null)
        {
            throw new InvalidOperationException($"Product with ID {product.Id} not found.");
        }
        
        // Check if name is already taken by another product
        var nameConflict = await _productRepository.GetByNameAsync(product.Name, cancellationToken);
        if (nameConflict != null && nameConflict.Id != product.Id)
        {
            throw new InvalidOperationException($"A product with name '{product.Name}' already exists.");
        }
        
        // Repository will handle validation
        var updatedProduct = await _productRepository.UpdateAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        
        return updatedProduct;
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {id} not found.");
        }
        
        await _productRepository.DeleteAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetByCategory(category, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetLowStockProducts(threshold, cancellationToken);
    }

    public async Task<Product> UpdateProductPriceAsync(Guid id, decimal newPrice, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {id} not found.");
        }
        
        product.UpdatePrice(newPrice);
        
        // Repository will handle validation
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        
        return product;
    }

    public async Task<Product> UpdateProductQuantityAsync(Guid id, int newQuantity, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {id} not found.");
        }
        
        product.UpdateQuantity(newQuantity);
        
        // Repository will handle validation
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        
        return product;
    }

    public async Task<decimal> GetInventoryValueAsync(CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetTotalInventoryValue(cancellationToken);
    }

    public async Task<bool> IsProductNameAvailableAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var existingProduct = await _productRepository.GetByNameAsync(name, cancellationToken);
        return existingProduct == null || (excludeId.HasValue && existingProduct.Id == excludeId.Value);
    }
}