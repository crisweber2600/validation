using System;
using Microsoft.Extensions.Configuration;

namespace Validation.Domain.Providers;

/// <summary>
/// Interface for providing application names for multi-tenant scenarios
/// </summary>
public interface IApplicationNameProvider
{
    /// <summary>
    /// Get the current application name
    /// </summary>
    string GetApplicationName();
    
    /// <summary>
    /// Get the application name for a specific context
    /// </summary>
    string GetApplicationName(string? context);
}

/// <summary>
/// Configuration-based application name provider
/// </summary>
public class ConfigurationApplicationNameProvider : IApplicationNameProvider
{
    private readonly IConfiguration _configuration;
    private readonly string _defaultApplicationName;

    public ConfigurationApplicationNameProvider(IConfiguration configuration, string defaultApplicationName = "ValidationApp")
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _defaultApplicationName = defaultApplicationName;
    }

    public string GetApplicationName()
    {
        return _configuration["Application:Name"] 
            ?? _configuration["ApplicationName"] 
            ?? _defaultApplicationName;
    }

    public string GetApplicationName(string? context)
    {
        if (string.IsNullOrEmpty(context))
            return GetApplicationName();

        return _configuration[$"Application:Name:{context}"] 
            ?? _configuration[$"ApplicationName:{context}"] 
            ?? GetApplicationName();
    }
}

/// <summary>
/// Environment-based application name provider
/// </summary>
public class EnvironmentApplicationNameProvider : IApplicationNameProvider
{
    private readonly string _defaultApplicationName;

    public EnvironmentApplicationNameProvider(string defaultApplicationName = "ValidationApp")
    {
        _defaultApplicationName = defaultApplicationName;
    }

    public string GetApplicationName()
    {
        return Environment.GetEnvironmentVariable("APPLICATION_NAME") 
            ?? Environment.GetEnvironmentVariable("APP_NAME") 
            ?? _defaultApplicationName;
    }

    public string GetApplicationName(string? context)
    {
        if (string.IsNullOrEmpty(context))
            return GetApplicationName();

        return Environment.GetEnvironmentVariable($"APPLICATION_NAME_{context}") 
            ?? Environment.GetEnvironmentVariable($"APP_NAME_{context}") 
            ?? GetApplicationName();
    }
}

/// <summary>
/// Static application name provider for simple scenarios
/// </summary>
public class StaticApplicationNameProvider : IApplicationNameProvider
{
    private readonly string _applicationName;

    public StaticApplicationNameProvider(string applicationName)
    {
        _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
    }

    public string GetApplicationName() => _applicationName;

    public string GetApplicationName(string? context) => _applicationName;
}

/// <summary>
/// Composite application name provider that tries multiple providers in order
/// </summary>
public class CompositeApplicationNameProvider : IApplicationNameProvider
{
    private readonly IApplicationNameProvider[] _providers;

    public CompositeApplicationNameProvider(params IApplicationNameProvider[] providers)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
    }

    public string GetApplicationName()
    {
        foreach (var provider in _providers)
        {
            try
            {
                var name = provider.GetApplicationName();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            catch
            {
                // Continue to next provider
            }
        }

        return "ValidationApp"; // Fallback
    }

    public string GetApplicationName(string? context)
    {
        foreach (var provider in _providers)
        {
            try
            {
                var name = provider.GetApplicationName(context);
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            catch
            {
                // Continue to next provider
            }
        }

        return GetApplicationName(); // Fallback to context-less
    }
}