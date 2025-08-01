using System.ComponentModel.DataAnnotations;

namespace Validation.RepositoryPattern.Sample.Models;

/// <summary>
/// Sample Customer entity for demonstrating repository pattern integration with validation
/// </summary>
public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? Phone { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    public decimal CreditLimit { get; set; }
    
    public decimal CurrentBalance { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string FullName => $"{FirstName} {LastName}";
    
    public decimal AvailableCredit => CreditLimit - CurrentBalance;
    
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
    
    public bool CanPurchase(decimal amount) => AvailableCredit >= amount;
}