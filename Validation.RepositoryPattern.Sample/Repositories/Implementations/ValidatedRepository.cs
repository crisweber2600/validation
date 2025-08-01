using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.RepositoryPattern.Sample.Data;

namespace Validation.RepositoryPattern.Sample.Repositories.Implementations;

/// <summary>
/// Enhanced repository that integrates validation into repository operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class ValidatedRepository<T> : Repository<T> where T : class
{
    private readonly IEnhancedManualValidatorService _validator;
    private readonly ILogger<ValidatedRepository<T>> _logger;

    public ValidatedRepository(
        SampleDbContext context, 
        IEnhancedManualValidatorService validator,
        ILogger<ValidatedRepository<T>> logger) : base(context)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        _logger.LogInformation("Validating entity before add: {EntityType}", typeof(T).Name);
        
        // Perform validation before adding
        var validationResult = await _validator.ValidateAsync(entity);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Validation failed for {typeof(T).Name}";
            _logger.LogWarning("{ErrorMessage}. Failed rules: {FailedRules}", 
                errorMessage, 
                string.Join(", ", validationResult.FailedRules ?? new List<string>()));
            
            throw new ValidationException(errorMessage, validationResult.FailedRules, validationResult.Errors);
        }
        
        _logger.LogInformation("Validation successful for {EntityType}", typeof(T).Name);
        return await base.AddAsync(entity, cancellationToken);
    }

    public override async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        
        var entitiesList = entities.ToList();
        _logger.LogInformation("Validating {Count} entities before bulk add: {EntityType}", 
            entitiesList.Count, typeof(T).Name);
        
        var failedValidations = new List<(T Entity, ValidationResult Result)>();
        
        foreach (var entity in entitiesList)
        {
            var validationResult = await _validator.ValidateAsync(entity);
            if (!validationResult.IsValid)
            {
                failedValidations.Add((entity, validationResult));
            }
        }
        
        if (failedValidations.Any())
        {
            var errorMessage = $"Validation failed for {failedValidations.Count} out of {entitiesList.Count} {typeof(T).Name} entities";
            var allFailedRules = failedValidations.SelectMany(f => f.Result.FailedRules ?? new List<string>()).Distinct().ToList();
            var allErrors = failedValidations.SelectMany(f => f.Result.Errors ?? new List<string>()).ToList();
            
            _logger.LogWarning("{ErrorMessage}. Failed rules: {FailedRules}", 
                errorMessage, string.Join(", ", allFailedRules));
            
            throw new ValidationException(errorMessage, allFailedRules, allErrors);
        }
        
        _logger.LogInformation("Validation successful for all {Count} {EntityType} entities", 
            entitiesList.Count, typeof(T).Name);
        
        return await base.AddRangeAsync(entitiesList, cancellationToken);
    }

    public override async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        _logger.LogInformation("Validating entity before update: {EntityType}", typeof(T).Name);
        
        // Perform validation before updating
        var validationResult = await _validator.ValidateAsync(entity);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Validation failed for {typeof(T).Name} update";
            _logger.LogWarning("{ErrorMessage}. Failed rules: {FailedRules}", 
                errorMessage, 
                string.Join(", ", validationResult.FailedRules ?? new List<string>()));
            
            throw new ValidationException(errorMessage, validationResult.FailedRules, validationResult.Errors);
        }
        
        _logger.LogInformation("Validation successful for {EntityType} update", typeof(T).Name);
        return await base.UpdateAsync(entity, cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing pre-save validation for modified entities");
        
        // Get all modified entities and validate them
        var modifiedEntries = _context.ChangeTracker.Entries<T>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        
        foreach (var entry in modifiedEntries)
        {
            var validationResult = await _validator.ValidateAsync(entry.Entity);
            if (!validationResult.IsValid)
            {
                var errorMessage = $"Pre-save validation failed for {typeof(T).Name}";
                _logger.LogWarning("{ErrorMessage}. Failed rules: {FailedRules}", 
                    errorMessage, 
                    string.Join(", ", validationResult.FailedRules ?? new List<string>()));
                
                throw new ValidationException(errorMessage, validationResult.FailedRules, validationResult.Errors);
            }
        }
        
        _logger.LogInformation("Pre-save validation successful for {Count} modified entities", modifiedEntries.Count);
        
        var result = await base.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully saved {Count} changes to database", result);
        return result;
    }
}

/// <summary>
/// Custom validation exception for repository operations
/// </summary>
public class ValidationException : Exception
{
    public IEnumerable<string> FailedRules { get; }
    public IEnumerable<string> ValidationErrors { get; }

    public ValidationException(string message, IEnumerable<string>? failedRules = null, IEnumerable<string>? errors = null) 
        : base(message)
    {
        FailedRules = failedRules ?? new List<string>();
        ValidationErrors = errors ?? new List<string>();
    }
}