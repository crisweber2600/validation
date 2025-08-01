using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.RepositoryPattern.Sample.Data;
using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories.Implementations;

/// <summary>
/// Validated Customer repository that integrates validation with customer-specific operations
/// </summary>
public class ValidatedCustomerRepository : ValidatedRepository<Customer>, ICustomerRepository
{
    public ValidatedCustomerRepository(
        SampleDbContext context, 
        IEnhancedManualValidatorService validator,
        ILogger<ValidatedCustomerRepository> logger) : base(context, validator, logger)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomers(CancellationToken cancellationToken = default)
    {
        return await FindAsync(c => c.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersWithCreditLimit(decimal minLimit, CancellationToken cancellationToken = default)
    {
        return await FindAsync(c => c.IsActive && c.CreditLimit >= minLimit, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByAge(int minAge, int maxAge, CancellationToken cancellationToken = default)
    {
        var customers = await GetActiveCustomers(cancellationToken);
        return customers.Where(c => c.Age >= minAge && c.Age <= maxAge);
    }

    public async Task<decimal> GetTotalCreditExposure(CancellationToken cancellationToken = default)
    {
        var activeCustomers = await GetActiveCustomers(cancellationToken);
        return activeCustomers.Sum(c => c.CurrentBalance);
    }
}