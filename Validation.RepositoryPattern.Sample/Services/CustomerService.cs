using Validation.RepositoryPattern.Sample.Models;
using Validation.RepositoryPattern.Sample.Repositories;

namespace Validation.RepositoryPattern.Sample.Services;

/// <summary>
/// Customer service interface for business operations
/// </summary>
public interface ICustomerService
{
    // Basic operations
    Task<Customer?> GetCustomerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
    Task<Customer> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer> UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Business operations
    Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetHighValueCustomersAsync(decimal minCreditLimit, CancellationToken cancellationToken = default);
    Task<Customer> UpdateCreditLimitAsync(Guid id, decimal newCreditLimit, CancellationToken cancellationToken = default);
    Task<Customer> ProcessPaymentAsync(Guid id, decimal amount, CancellationToken cancellationToken = default);
    Task<Customer> AddChargeAsync(Guid id, decimal amount, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalCreditExposureAsync(CancellationToken cancellationToken = default);
    Task<bool> IsEmailAvailableAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Customer service implementation using repository pattern with validation
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public async Task<Customer?> GetCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _customerRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _customerRepository.GetActiveCustomers(cancellationToken);
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        
        // Check if email is already taken
        var existingCustomer = await _customerRepository.GetByEmailAsync(customer.Email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"A customer with email '{customer.Email}' already exists.");
        }
        
        // Repository will handle validation
        var addedCustomer = await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        
        return addedCustomer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        
        var existingCustomer = await _customerRepository.GetByIdAsync(customer.Id, cancellationToken);
        if (existingCustomer == null)
        {
            throw new InvalidOperationException($"Customer with ID {customer.Id} not found.");
        }
        
        // Check if email is already taken by another customer
        var emailConflict = await _customerRepository.GetByEmailAsync(customer.Email, cancellationToken);
        if (emailConflict != null && emailConflict.Id != customer.Id)
        {
            throw new InvalidOperationException($"A customer with email '{customer.Email}' already exists.");
        }
        
        customer.UpdatedAt = DateTime.UtcNow;
        
        // Repository will handle validation
        var updatedCustomer = await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        
        return updatedCustomer;
    }

    public async Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {id} not found.");
        }
        
        await _customerRepository.DeleteAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _customerRepository.GetByEmailAsync(email, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetHighValueCustomersAsync(decimal minCreditLimit, CancellationToken cancellationToken = default)
    {
        return await _customerRepository.GetCustomersWithCreditLimit(minCreditLimit, cancellationToken);
    }

    public async Task<Customer> UpdateCreditLimitAsync(Guid id, decimal newCreditLimit, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {id} not found.");
        }
        
        if (newCreditLimit < 0)
        {
            throw new ArgumentException("Credit limit cannot be negative.", nameof(newCreditLimit));
        }
        
        if (newCreditLimit < customer.CurrentBalance)
        {
            throw new InvalidOperationException($"Credit limit cannot be less than current balance of {customer.CurrentBalance:C}.");
        }
        
        customer.CreditLimit = newCreditLimit;
        customer.UpdatedAt = DateTime.UtcNow;
        
        // Repository will handle validation
        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        
        return customer;
    }

    public async Task<Customer> ProcessPaymentAsync(Guid id, decimal amount, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {id} not found.");
        }
        
        if (amount <= 0)
        {
            throw new ArgumentException("Payment amount must be positive.", nameof(amount));
        }
        
        if (amount > customer.CurrentBalance)
        {
            throw new InvalidOperationException($"Payment amount {amount:C} exceeds current balance of {customer.CurrentBalance:C}.");
        }
        
        customer.CurrentBalance -= amount;
        customer.UpdatedAt = DateTime.UtcNow;
        
        // Repository will handle validation
        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        
        return customer;
    }

    public async Task<Customer> AddChargeAsync(Guid id, decimal amount, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {id} not found.");
        }
        
        if (amount <= 0)
        {
            throw new ArgumentException("Charge amount must be positive.", nameof(amount));
        }
        
        var newBalance = customer.CurrentBalance + amount;
        if (newBalance > customer.CreditLimit)
        {
            throw new InvalidOperationException($"Charge would exceed credit limit. Available credit: {customer.AvailableCredit:C}");
        }
        
        customer.CurrentBalance = newBalance;
        customer.UpdatedAt = DateTime.UtcNow;
        
        // Repository will handle validation
        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        
        return customer;
    }

    public async Task<decimal> GetTotalCreditExposureAsync(CancellationToken cancellationToken = default)
    {
        return await _customerRepository.GetTotalCreditExposure(cancellationToken);
    }

    public async Task<bool> IsEmailAvailableAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var existingCustomer = await _customerRepository.GetByEmailAsync(email, cancellationToken);
        return existingCustomer == null || (excludeId.HasValue && existingCustomer.Id == excludeId.Value);
    }
}