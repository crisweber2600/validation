using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Validation.Domain.Validation;
using Validation.Infrastructure.Reliability;
using Validation.Infrastructure.Metrics;
using Validation.Infrastructure.Auditing;

namespace Validation.Infrastructure.Setup;

public interface IValidationSetupService
{
    Task<SetupValidationResult> ValidateSetupAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateConfigurationAsync(CancellationToken cancellationToken = default);
    Task<HealthCheckResult> PerformHealthChecksAsync(CancellationToken cancellationToken = default);
}

public class ValidationSetupService : IValidationSetupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationSetupService> _logger;
    private readonly ValidationSetupOptions _options;

    public ValidationSetupService(
        IServiceProvider serviceProvider,
        ILogger<ValidationSetupService> logger,
        IOptions<ValidationSetupOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<SetupValidationResult> ValidateSetupAsync(CancellationToken cancellationToken = default)
    {
        var result = new SetupValidationResult();

        try
        {
            _logger.LogInformation("Starting validation setup checks");

            // Check service registrations
            await ValidateServiceRegistrationsAsync(result, cancellationToken);

            // Check configuration
            await ValidateConfigurationInternalAsync(result, cancellationToken);

            // Check dependencies
            await ValidateDependenciesAsync(result, cancellationToken);

            // Perform health checks
            var healthCheck = await PerformHealthChecksAsync(cancellationToken);
            result.IsHealthy = healthCheck.IsHealthy;
            result.HealthDetails.AddRange(healthCheck.Details);

            result.IsValid = result.ServiceRegistrationIssues.Count == 0 &&
                           result.ConfigurationIssues.Count == 0 &&
                           result.DependencyIssues.Count == 0 &&
                           result.IsHealthy;

            _logger.LogInformation("Validation setup checks completed. Result: {IsValid}", result.IsValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during validation setup checks");
            result.IsValid = false;
            result.ConfigurationIssues.Add($"Setup validation failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> ValidateConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var result = new SetupValidationResult();
        await ValidateConfigurationInternalAsync(result, cancellationToken);
        return result.ConfigurationIssues.Count == 0;
    }

    public async Task<HealthCheckResult> PerformHealthChecksAsync(CancellationToken cancellationToken = default)
    {
        var result = new HealthCheckResult { IsHealthy = true };

        try
        {
            // Check validation plan provider
            await CheckValidationPlanProviderAsync(result, cancellationToken);

            // Check manual validator service
            await CheckManualValidatorServiceAsync(result, cancellationToken);

            // Check metrics collector
            await CheckMetricsCollectorAsync(result, cancellationToken);

            // Check reliability policies
            await CheckReliabilityPoliciesAsync(result, cancellationToken);

            result.IsHealthy = result.Details.All(d => d.IsHealthy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health checks");
            result.IsHealthy = false;
            result.Details.Add(new HealthDetail { Name = "HealthCheck", IsHealthy = false, Message = ex.Message });
        }

        return result;
    }

    private async Task ValidateServiceRegistrationsAsync(SetupValidationResult result, CancellationToken cancellationToken)
    {
        var requiredServices = new[]
        {
            typeof(IValidationPlanProvider),
            typeof(IManualValidatorService),
            typeof(IEnhancedManualValidatorService),
            typeof(IMetricsCollector),
            typeof(DeletePipelineReliabilityPolicy),
            typeof(NannyRecordAuditService)
        };

        foreach (var serviceType in requiredServices)
        {
            try
            {
                var service = _serviceProvider.GetService(serviceType);
                if (service == null)
                {
                    result.ServiceRegistrationIssues.Add($"Required service {serviceType.Name} is not registered");
                }
            }
            catch (Exception ex)
            {
                result.ServiceRegistrationIssues.Add($"Error resolving service {serviceType.Name}: {ex.Message}");
            }
        }

        await Task.CompletedTask;
    }

    private async Task ValidateConfigurationInternalAsync(SetupValidationResult result, CancellationToken cancellationToken)
    {
        // Validate DeleteReliabilityOptions
        try
        {
            var deleteOptions = _serviceProvider.GetService<DeleteReliabilityOptions>();
            if (deleteOptions != null)
            {
                if (deleteOptions.MaxRetryAttempts <= 0)
                    result.ConfigurationIssues.Add("DeleteReliabilityOptions.MaxRetryAttempts must be greater than 0");
                if (deleteOptions.RetryDelayMs <= 0)
                    result.ConfigurationIssues.Add("DeleteReliabilityOptions.RetryDelayMs must be greater than 0");
            }
        }
        catch (Exception ex)
        {
            result.ConfigurationIssues.Add($"Error validating DeleteReliabilityOptions: {ex.Message}");
        }

        // Validate MetricsOrchestratorOptions
        try
        {
            var metricsOptions = _serviceProvider.GetService<MetricsOrchestratorOptions>();
            if (metricsOptions != null)
            {
                if (metricsOptions.ProcessingIntervalMs <= 0)
                    result.ConfigurationIssues.Add("MetricsOrchestratorOptions.ProcessingIntervalMs must be greater than 0");
            }
        }
        catch (Exception ex)
        {
            result.ConfigurationIssues.Add($"Error validating MetricsOrchestratorOptions: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private async Task ValidateDependenciesAsync(SetupValidationResult result, CancellationToken cancellationToken)
    {
        // This would validate external dependencies like databases, message queues, etc.
        // For now, we'll just simulate the check
        await Task.Delay(100, cancellationToken);
    }

    private async Task CheckValidationPlanProviderAsync(HealthCheckResult result, CancellationToken cancellationToken)
    {
        try
        {
            var provider = _serviceProvider.GetRequiredService<IValidationPlanProvider>();
            var rules = provider.GetRules<object>();
            result.Details.Add(new HealthDetail 
            { 
                Name = "ValidationPlanProvider", 
                IsHealthy = true, 
                Message = $"Available, {rules.Count()} rules configured" 
            });
        }
        catch (Exception ex)
        {
            result.Details.Add(new HealthDetail 
            { 
                Name = "ValidationPlanProvider", 
                IsHealthy = false, 
                Message = ex.Message 
            });
        }

        await Task.CompletedTask;
    }

    private async Task CheckManualValidatorServiceAsync(HealthCheckResult result, CancellationToken cancellationToken)
    {
        try
        {
            var validator = _serviceProvider.GetRequiredService<IEnhancedManualValidatorService>();
            var testResult = validator.ValidateWithDetails(new object());
            result.Details.Add(new HealthDetail 
            { 
                Name = "ManualValidatorService", 
                IsHealthy = true, 
                Message = "Service is responsive" 
            });
        }
        catch (Exception ex)
        {
            result.Details.Add(new HealthDetail 
            { 
                Name = "ManualValidatorService", 
                IsHealthy = false, 
                Message = ex.Message 
            });
        }

        await Task.CompletedTask;
    }

    private async Task CheckMetricsCollectorAsync(HealthCheckResult result, CancellationToken cancellationToken)
    {
        try
        {
            var metricsCollector = _serviceProvider.GetRequiredService<IMetricsCollector>();
            var summary = await metricsCollector.GetMetricsSummaryAsync(TimeSpan.FromMinutes(1));
            result.Details.Add(new HealthDetail 
            { 
                Name = "MetricsCollector", 
                IsHealthy = true, 
                Message = $"Available, {summary.TotalValidations} recent validations" 
            });
        }
        catch (Exception ex)
        {
            result.Details.Add(new HealthDetail 
            { 
                Name = "MetricsCollector", 
                IsHealthy = false, 
                Message = ex.Message 
            });
        }
    }

    private async Task CheckReliabilityPoliciesAsync(HealthCheckResult result, CancellationToken cancellationToken)
    {
        try
        {
            var policy = _serviceProvider.GetRequiredService<DeletePipelineReliabilityPolicy>();
            result.Details.Add(new HealthDetail 
            { 
                Name = "ReliabilityPolicies", 
                IsHealthy = true, 
                Message = "Delete pipeline reliability policy is available" 
            });
        }
        catch (Exception ex)
        {
            result.Details.Add(new HealthDetail 
            { 
                Name = "ReliabilityPolicies", 
                IsHealthy = false, 
                Message = ex.Message 
            });
        }

        await Task.CompletedTask;
    }
}

public class SetupValidationResult
{
    public bool IsValid { get; set; }
    public bool IsHealthy { get; set; }
    public List<string> ServiceRegistrationIssues { get; set; } = new();
    public List<string> ConfigurationIssues { get; set; } = new();
    public List<string> DependencyIssues { get; set; } = new();
    public List<HealthDetail> HealthDetails { get; set; } = new();
    
    public string GetSummary()
    {
        if (IsValid) return "Setup validation passed";
        
        var issues = new List<string>();
        issues.AddRange(ServiceRegistrationIssues);
        issues.AddRange(ConfigurationIssues);
        issues.AddRange(DependencyIssues);
        
        return $"Setup validation failed. Issues: {string.Join(", ", issues)}";
    }
}

public class HealthCheckResult
{
    public bool IsHealthy { get; set; }
    public List<HealthDetail> Details { get; set; } = new();
}

public class HealthDetail
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ValidationSetupOptions
{
    public bool EnableStartupValidation { get; set; } = true;
    public bool FailOnValidationErrors { get; set; } = false;
    public TimeSpan HealthCheckTimeout { get; set; } = TimeSpan.FromSeconds(30);
}