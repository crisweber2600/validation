using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Metrics;
using Validation.Infrastructure.Reliability;
using Validation.Infrastructure.Auditing;

namespace Validation.Infrastructure.Setup;

/// <summary>
/// Fluent builder for setting up validation system with comprehensive configuration
/// </summary>
public class SetupValidationBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<ValidationFlowConfig> _flowConfigs = new();
    private readonly List<Action<IServiceCollection>> _customRegistrations = new();
    private bool _metricsEnabled = true;
    private bool _auditingEnabled = true;
    private bool _reliabilityEnabled = true;
    private bool _observabilityEnabled = true;

    public SetupValidationBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Configure database storage using Entity Framework
    /// </summary>
    public SetupValidationBuilder UseEntityFramework<TContext>(Action<DbContextOptionsBuilder>? configureOptions = null)
        where TContext : DbContext
    {
        _customRegistrations.Add(services =>
        {
            if (configureOptions != null)
            {
                services.AddDbContext<TContext>(configureOptions);
            }
            else
            {
                services.AddDbContext<TContext>(options => options.UseInMemoryDatabase("ValidationDb"));
            }
            services.AddScoped<DbContext, TContext>();
        });
        return this;
    }

    /// <summary>
    /// Configure database storage using MongoDB
    /// </summary>
    public SetupValidationBuilder UseMongoDB(string connectionString, string databaseName)
    {
        _customRegistrations.Add(services =>
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            services.AddSingleton(database);
        });
        return this;
    }

    /// <summary>
    /// Add a validation flow for a specific entity type
    /// </summary>
    public SetupValidationBuilder AddValidationFlow<T>(Action<ValidationFlowBuilder<T>>? configure = null)
    {
        var builder = new ValidationFlowBuilder<T>();
        configure?.Invoke(builder);
        _flowConfigs.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Add a manual validation rule
    /// </summary>
    public SetupValidationBuilder AddRule<T>(Func<T, bool> rule)
    {
        _customRegistrations.Add(services =>
        {
            services.AddValidatorRule(rule);
        });
        return this;
    }

    /// <summary>
    /// Add a named validation rule
    /// </summary>
    public SetupValidationBuilder AddRule<T>(string ruleName, Func<T, bool> rule)
    {
        _customRegistrations.Add(services =>
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEnhancedManualValidatorService));
            if (descriptor?.ImplementationInstance is EnhancedManualValidatorService existing)
            {
                existing.AddRule(ruleName, rule);
            }
            else
            {
                // Will be registered later
                services.AddSingleton<Action<IEnhancedManualValidatorService>>(validator =>
                {
                    validator.AddRule(ruleName, rule);
                });
            }
        });
        return this;
    }

    /// <summary>
    /// Configure metrics collection
    /// </summary>
    public SetupValidationBuilder ConfigureMetrics(Action<MetricsBuilder>? configure = null)
    {
        var builder = new MetricsBuilder();
        configure?.Invoke(builder);
        _customRegistrations.Add(builder.Configure);
        return this;
    }

    /// <summary>
    /// Disable metrics collection
    /// </summary>
    public SetupValidationBuilder DisableMetrics()
    {
        _metricsEnabled = false;
        return this;
    }

    /// <summary>
    /// Configure reliability policies
    /// </summary>
    public SetupValidationBuilder ConfigureReliability(Action<ReliabilityBuilder>? configure = null)
    {
        var builder = new ReliabilityBuilder();
        configure?.Invoke(builder);
        _customRegistrations.Add(builder.Configure);
        return this;
    }

    /// <summary>
    /// Disable reliability policies
    /// </summary>
    public SetupValidationBuilder DisableReliability()
    {
        _reliabilityEnabled = false;
        return this;
    }

    /// <summary>
    /// Configure auditing
    /// </summary>
    public SetupValidationBuilder ConfigureAuditing(Action<AuditingBuilder>? configure = null)
    {
        var builder = new AuditingBuilder();
        configure?.Invoke(builder);
        _customRegistrations.Add(builder.Configure);
        return this;
    }

    /// <summary>
    /// Disable auditing
    /// </summary>
    public SetupValidationBuilder DisableAuditing()
    {
        _auditingEnabled = false;
        return this;
    }

    /// <summary>
    /// Disable observability (OpenTelemetry)
    /// </summary>
    public SetupValidationBuilder DisableObservability()
    {
        _observabilityEnabled = false;
        return this;
    }

    /// <summary>
    /// Add custom service registrations
    /// </summary>
    public SetupValidationBuilder AddServices(Action<IServiceCollection> configure)
    {
        _customRegistrations.Add(configure);
        return this;
    }

    /// <summary>
    /// Build and register all validation services
    /// </summary>
    public IServiceCollection Build()
    {
        // Register core infrastructure
        if (_metricsEnabled || _auditingEnabled || _reliabilityEnabled || _observabilityEnabled)
        {
            _services.AddValidationInfrastructure();
        }

        // Register validation flows
        if (_flowConfigs.Count > 0)
        {
            _services.AddValidationFlows(_flowConfigs);
        }

        // Apply custom registrations
        foreach (var registration in _customRegistrations)
        {
            registration(_services);
        }

        // Configure post-registration actions for enhanced validator
        var postActions = _services.Where(s => s.ServiceType == typeof(Action<IEnhancedManualValidatorService>))
                                  .Select(s => (Action<IEnhancedManualValidatorService>)s.ImplementationInstance!)
                                  .ToList();

        if (postActions.Any())
        {
            _services.AddSingleton<IHostedService>(provider =>
            {
                var validator = provider.GetRequiredService<IEnhancedManualValidatorService>();
                foreach (var action in postActions)
                {
                    action(validator);
                }
                return new ValidationSetupHostedService();
            });
        }

        return _services;
    }
}

/// <summary>
/// Builder for configuring validation flows for specific entity types
/// </summary>
public class ValidationFlowBuilder<T>
{
    private readonly ValidationFlowConfig _config = new() { Type = typeof(T).AssemblyQualifiedName! };

    public ValidationFlowBuilder<T> EnableSaveValidation(bool enabled = true)
    {
        _config.SaveValidation = enabled;
        return this;
    }

    public ValidationFlowBuilder<T> EnableSaveCommit(bool enabled = true)
    {
        _config.SaveCommit = enabled;
        return this;
    }

    public ValidationFlowBuilder<T> EnableDeleteValidation(bool enabled = true)
    {
        _config.DeleteValidation = enabled;
        return this;
    }

    public ValidationFlowBuilder<T> EnableDeleteCommit(bool enabled = true)
    {
        _config.DeleteCommit = enabled;
        return this;
    }

    public ValidationFlowBuilder<T> EnableSoftDelete(bool enabled = true)
    {
        _config.SoftDeleteSupport = enabled;
        return this;
    }

    public ValidationFlowBuilder<T> WithValidationTimeout(TimeSpan timeout)
    {
        _config.ValidationTimeout = timeout;
        return this;
    }

    public ValidationFlowBuilder<T> WithMaxRetryAttempts(int maxRetries)
    {
        _config.MaxRetryAttempts = maxRetries;
        return this;
    }

    public ValidationFlowBuilder<T> EnableAuditing(bool enabled = true)
    {
        _config.EnableAuditing = enabled;
        return this;
    }

    public ValidationFlowBuilder<T> EnableMetrics(bool enabled = true)
    {
        _config.EnableMetrics = enabled;
        return this;
    }

    public ValidationFlowBuilder<T> WithThreshold<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ThresholdType thresholdType,
        decimal thresholdValue)
    {
        if (propertySelector.Body is MemberExpression member)
        {
            _config.MetricProperty = member.Member.Name;
            _config.ThresholdType = thresholdType;
            _config.ThresholdValue = thresholdValue;
        }
        return this;
    }

    public ValidationFlowConfig Build() => _config;
}

/// <summary>
/// Builder for configuring metrics collection
/// </summary>
public class MetricsBuilder
{
    private MetricsOrchestratorOptions _options = new();

    public MetricsBuilder WithProcessingInterval(TimeSpan interval)
    {
        _options.ProcessingIntervalMs = (int)interval.TotalMilliseconds;
        return this;
    }

    public MetricsBuilder EnableDetailedMetrics(bool enabled = true)
    {
        _options.LogDetailedMetrics = enabled;
        return this;
    }

    internal void Configure(IServiceCollection services)
    {
        services.AddSingleton(_options);
    }
}

/// <summary>
/// Builder for configuring reliability policies
/// </summary>
public class ReliabilityBuilder
{
    private DeleteReliabilityOptions _options = new();

    public ReliabilityBuilder WithMaxRetries(int maxRetries)
    {
        _options.MaxRetryAttempts = maxRetries;
        return this;
    }

    public ReliabilityBuilder WithRetryDelay(TimeSpan delay)
    {
        _options.RetryDelayMs = (int)delay.TotalMilliseconds;
        return this;
    }

    public ReliabilityBuilder WithCircuitBreaker(int threshold, TimeSpan timeout)
    {
        _options.CircuitBreakerThreshold = threshold;
        _options.CircuitBreakerTimeoutMs = (int)timeout.TotalMilliseconds;
        return this;
    }

    internal void Configure(IServiceCollection services)
    {
        services.AddSingleton(_options);
    }
}

/// <summary>
/// Builder for configuring auditing
/// </summary>
public class AuditingBuilder
{
    private NannyRecordAuditOptions _options = new();

    public AuditingBuilder EnableDetailedAuditing(bool enabled = true)
    {
        _options.EnableDetailedAuditing = enabled;
        return this;
    }

    public AuditingBuilder WithRetentionPeriod(TimeSpan retention)
    {
        _options.RetentionPeriod = retention;
        return this;
    }

    internal void Configure(IServiceCollection services)
    {
        services.AddSingleton(_options);
    }
}

/// <summary>
/// Minimal hosted service for post-setup initialization
/// </summary>
internal class ValidationSetupHostedService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// Extension methods for easy setup
/// </summary>
public static class SetupValidationExtensions
{
    /// <summary>
    /// Add validation system with fluent configuration
    /// </summary>
    public static SetupValidationBuilder AddSetupValidation(this IServiceCollection services)
    {
        return new SetupValidationBuilder(services);
    }

    /// <summary>
    /// Add validation system with basic configuration
    /// </summary>
    public static IServiceCollection AddValidation(this IServiceCollection services, Action<SetupValidationBuilder>? configure = null)
    {
        var builder = services.AddSetupValidation();
        configure?.Invoke(builder);
        return builder.Build();
    }
}