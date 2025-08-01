using Microsoft.EntityFrameworkCore;
using Validation.RepositoryPattern.Sample.Data;
using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories.Implementations;

/// <summary>
/// Customer repository implementation with customer-specific operations
/// </summary>
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomers(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersWithCreditLimit(decimal minLimit, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && c.CreditLimit >= minLimit)
            .OrderByDescending(c => c.CreditLimit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByAge(int minAge, int maxAge, CancellationToken cancellationToken = default)
    {
        var maxDate = DateTime.UtcNow.AddYears(-minAge);
        var minDate = DateTime.UtcNow.AddYears(-maxAge - 1);
        
        return await _dbSet
            .Where(c => c.IsActive && c.DateOfBirth <= maxDate && c.DateOfBirth > minDate)
            .OrderBy(c => c.DateOfBirth)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalCreditExposure(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .SumAsync(c => c.CurrentBalance, cancellationToken);
    }
}