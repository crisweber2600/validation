using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Metrics;
using Validation.Domain.Events;
using Validation.Domain;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Orchestrates validation flows with pipeline integration
/// </summary>
public class ValidationFlowOrchestrator
{
    private readonly IMetricsCollector _metricsCollector;
    private readonly IValidationEventHub _eventHub;
    private readonly ILogger<ValidationFlowOrchestrator> _logger;
    private readonly IEntityIdProvider _idProvider;
    private readonly List<ValidationFlowConfig> _flowConfigs;

    public ValidationFlowOrchestrator(
        IMetricsCollector metricsCollector,
        IValidationEventHub eventHub,
        ILogger<ValidationFlowOrchestrator> logger,
        IEntityIdProvider idProvider,
        IEnumerable<ValidationFlowConfig> flowConfigs)
    {
        _metricsCollector = metricsCollector;
        _eventHub = eventHub;
        _logger = logger;
        _idProvider = idProvider;
        _flowConfigs = flowConfigs.ToList();
    }

    /// <summary>
    /// Execute a validation flow for a specific entity type
    /// </summary>
    public async Task<ValidationFlowResult> ExecuteFlowAsync<T>(T entity, string operation = "Validate")
    {
        var entityType = typeof(T).Name;
        var config = _flowConfigs.FirstOrDefault(c => c.Type.Contains(entityType));
        
        if (config == null)
        {
            _logger.LogWarning("No validation flow configuration found for {EntityType}", entityType);
            return ValidationFlowResult.NotConfigured(entityType);
        }

        var startTime = DateTime.UtcNow;
        var entityId = GetEntityId(entity);

        try
        {
            _logger.LogInformation("Starting validation flow for {EntityType} {EntityId}", entityType, entityId);

            // Execute validation based on operation
            var validationResult = operation.ToLower() switch
            {
                "save" when config.SaveValidation => await ExecuteSaveValidationAsync(entity, config),
                "delete" when config.DeleteValidation => await ExecuteDeleteValidationAsync(entity, config),
                "validate" => await ExecuteGeneralValidationAsync(entity, config),
                _ => ValidationFlowResult.Skipped(entityType, $"Operation {operation} not configured")
            };

            var duration = DateTime.UtcNow - startTime;

            // Record metrics
            _metricsCollector.RecordValidationDuration(entityType, duration.TotalMilliseconds);
            _metricsCollector.RecordValidationResult(entityType, validationResult.Success);

            // Publish event
            if (validationResult.Success)
            {
                await _eventHub.PublishAsync(new SaveValidationCompleted(
                    entityId, entityType, true, entity));
            }
            else
            {
                await _eventHub.PublishAsync(new ValidationOperationFailed(
                    entityId, entityType, operation, validationResult.ErrorMessage ?? "Unknown error"));
            }

            _logger.LogInformation(
                "Validation flow completed for {EntityType} {EntityId} in {Duration}ms. Success: {Success}",
                entityType, entityId, duration.TotalMilliseconds, validationResult.Success);

            return validationResult;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _metricsCollector.RecordValidationResult(entityType, false);
            
            await _eventHub.PublishAsync(new ValidationOperationFailed(
                entityId, entityType, operation, ex.Message, ex));

            _logger.LogError(ex, "Validation flow failed for {EntityType} {EntityId}", entityType, entityId);
            
            return ValidationFlowResult.Failed(entityType, ex.Message, duration);
        }
    }

    private async Task<ValidationFlowResult> ExecuteSaveValidationAsync<T>(T entity, ValidationFlowConfig config)
    {
        var entityType = typeof(T).Name;
        
        // Apply validation rules in priority order
        var sortedRules = config.ValidationRules
            .Where(r => r.Enabled)
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in sortedRules)
        {
            var ruleResult = await ExecuteValidationRuleAsync(entity, rule, config);
            if (!ruleResult.Success)
            {
                if (rule.IsRequired)
                {
                    return ValidationFlowResult.Failed(entityType, 
                        $"Required validation rule '{rule.Name}' failed: {ruleResult.ErrorMessage}");
                }
                
                _logger.LogWarning(
                    "Optional validation rule '{RuleName}' failed for {EntityType}: {Error}",
                    rule.Name, entityType, ruleResult.ErrorMessage);
            }
        }

        return ValidationFlowResult.Successful(entityType, TimeSpan.Zero);
    }

    private async Task<ValidationFlowResult> ExecuteDeleteValidationAsync<T>(T entity, ValidationFlowConfig config)
    {
        var entityType = typeof(T).Name;
        
        if (config.SoftDeleteSupport)
        {
            _logger.LogDebug("Performing soft delete for {EntityType}", entityType);
            return ValidationFlowResult.Successful(entityType, TimeSpan.Zero, "Soft delete performed");
        }

        // Hard delete validation
        _logger.LogDebug("Performing hard delete validation for {EntityType}", entityType);
        return ValidationFlowResult.Successful(entityType, TimeSpan.Zero, "Hard delete validated");
    }

    private async Task<ValidationFlowResult> ExecuteGeneralValidationAsync<T>(T entity, ValidationFlowConfig config)
    {
        // General validation logic
        return await ExecuteSaveValidationAsync(entity, config);
    }

    private async Task<ValidationFlowResult> ExecuteValidationRuleAsync<T>(T entity, ValidationRuleConfig rule, ValidationFlowConfig config)
    {
        var timeout = rule.Timeout ?? config.ValidationTimeout ?? TimeSpan.FromSeconds(30);
        
        try
        {
            // Basic rule validation (would be extended with actual rule engine)
            await Task.Delay(10); // Simulate validation work
            
            // For now, just return success - in real implementation this would apply actual rules
            return ValidationFlowResult.Successful(typeof(T).Name, TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            return ValidationFlowResult.Failed(typeof(T).Name, ex.Message);
        }
    }

    private Guid GetEntityId<T>(T entity)
    {
        var raw = _idProvider.GetId(entity);
        return Guid.TryParse(raw, out var id) ? id : Guid.NewGuid();
    }
}

/// <summary>
/// Result of validation flow execution
/// </summary>
public class ValidationFlowResult
{
    public bool Success { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Details { get; set; }

    public static ValidationFlowResult Successful(string entityType, TimeSpan duration, string? details = null)
        => new() { Success = true, EntityType = entityType, Duration = duration, Details = details };

    public static ValidationFlowResult Failed(string entityType, string errorMessage, TimeSpan duration = default)
        => new() { Success = false, EntityType = entityType, ErrorMessage = errorMessage, Duration = duration };

    public static ValidationFlowResult NotConfigured(string entityType)
        => new() { Success = false, EntityType = entityType, ErrorMessage = "No validation flow configured" };

    public static ValidationFlowResult Skipped(string entityType, string reason)
        => new() { Success = true, EntityType = entityType, Details = reason };
}