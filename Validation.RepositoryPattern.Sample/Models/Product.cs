using System.ComponentModel.DataAnnotations;

namespace Validation.RepositoryPattern.Sample.Models;

/// <summary>
/// Sample Product entity for demonstrating repository pattern integration with validation
/// </summary>
public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public int Quantity { get; set; }
    
    public string? Description { get; set; }
    
    public string Category { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsActive { get; set; } = true;

    public decimal GetTotalValue() => Price * Quantity;
    
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateQuantity(int newQuantity)
    {
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}