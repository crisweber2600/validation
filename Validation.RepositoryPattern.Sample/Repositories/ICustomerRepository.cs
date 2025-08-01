using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories;

/// <summary>
/// Customer-specific repository interface
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetActiveCustomers(CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetCustomersWithCreditLimit(decimal minLimit, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetCustomersByAge(int minAge, int maxAge, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalCreditExposure(CancellationToken cancellationToken = default);
}