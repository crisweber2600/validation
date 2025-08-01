using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Validation.Domain.Providers;

/// <summary>
/// Interface for extracting entity IDs from objects
/// </summary>
public interface IEntityIdProvider
{
    /// <summary>
    /// Extract the ID from an entity
    /// </summary>
    Guid GetEntityId<T>(T entity);
    
    /// <summary>
    /// Check if the provider can handle the given entity type
    /// </summary>
    bool CanHandle<T>();
}

/// <summary>
/// Reflection-based entity ID provider that automatically identifies entity IDs
/// </summary>
public class ReflectionEntityIdProvider : IEntityIdProvider
{
    private static readonly string[] IdPropertyNames = { "Id", "EntityId", "Guid", "Key" };

    public Guid GetEntityId<T>(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var type = typeof(T);
        
        // Try common ID property names
        foreach (var propertyName in IdPropertyNames)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                var value = property.GetValue(entity);
                if (value != null)
                {
                    // Handle different ID types
                    if (value is Guid guid)
                        return guid;
                    if (value is string str && Guid.TryParse(str, out var parsedGuid))
                        return parsedGuid;
                    if (value is int intId)
                        return new Guid(intId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    if (value is long longId)
                        return new Guid((int)(longId & 0xFFFFFFFF), (short)((longId >> 32) & 0xFFFF), 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }
            }
        }

        // If no ID property found, try to generate one based on object hash
        return new Guid(entity.GetHashCode(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    public bool CanHandle<T>()
    {
        var type = typeof(T);
        
        // Check if type has any of the common ID properties
        foreach (var propertyName in IdPropertyNames)
        {
            if (type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null)
                return true;
        }
        
        return true; // Can fallback to hash-based ID
    }
}

/// <summary>
/// Attribute-based entity ID provider that uses custom attributes to identify ID properties
/// </summary>
public class AttributeEntityIdProvider : IEntityIdProvider
{
    public Guid GetEntityId<T>(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Look for EntityIdAttribute or similar
            if (property.GetCustomAttribute<EntityIdAttribute>() != null)
            {
                var value = property.GetValue(entity);
                if (value is Guid guid)
                    return guid;
                if (value is string str && Guid.TryParse(str, out var parsedGuid))
                    return parsedGuid;
            }
        }

        // Fallback to reflection provider
        var reflectionProvider = new ReflectionEntityIdProvider();
        return reflectionProvider.GetEntityId(entity);
    }

    public bool CanHandle<T>()
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return properties.Any(p => p.GetCustomAttribute<EntityIdAttribute>() != null);
    }
}

/// <summary>
/// Attribute to mark properties as entity IDs
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EntityIdAttribute : Attribute
{
}

/// <summary>
/// Configurable entity ID provider that supports per-type custom selectors
/// </summary>
public class ConfigurableEntityIdProvider : IEntityIdProvider
{
    private readonly Dictionary<Type, Func<object, Guid>> _selectors = new();

    /// <summary>
    /// Register a custom selector for a specific entity type
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="selector">Function to extract string key from entity</param>
    public void RegisterSelector<T>(Func<T, string> selector)
    {
        _selectors[typeof(T)] = obj =>
        {
            var key = selector((T)obj) ?? string.Empty;
            // Create a deterministic Guid from the string (use MD5 hash)
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
            return new Guid(hash);
        };
    }

    public Guid GetEntityId<T>(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (_selectors.TryGetValue(typeof(T), out var extractor))
        {
            return extractor(entity);
        }

        // fallback: return Guid from entity.Id if available, otherwise new Guid
        var idProp = typeof(T).GetProperty("Id");
        if (idProp?.GetValue(entity) is Guid id) 
            return id;
        
        return Guid.NewGuid();
    }

    public bool CanHandle<T>()
    {
        return _selectors.ContainsKey(typeof(T)) || typeof(T).GetProperty("Id") != null;
    }
}

/// <summary>
/// Reflection-based entity ID provider with configurable property priority
/// </summary>
public class ReflectionBasedEntityIdProvider : IEntityIdProvider
{
    private readonly string[] _propertyPriority;

    public ReflectionBasedEntityIdProvider(params string[] propertyPriority)
    {
        _propertyPriority = propertyPriority.Length > 0 ? propertyPriority : new[] { "Name", "Code", "Key", "Title" };
    }

    public Guid GetEntityId<T>(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var type = typeof(T);
        
        // Try priority string properties first
        foreach (var propertyName in _propertyPriority)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property?.PropertyType == typeof(string))
            {
                var value = property.GetValue(entity) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    // Create deterministic Guid from string value
                    using var md5 = MD5.Create();
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                    return new Guid(hash);
                }
            }
        }

        // Fallback to existing Id property
        var idProperty = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty != null)
        {
            var value = idProperty.GetValue(entity);
            if (value is Guid guid)
                return guid;
            if (value is string str && Guid.TryParse(str, out var parsedGuid))
                return parsedGuid;
        }

        // Final fallback to new Guid
        return Guid.NewGuid();
    }

    public bool CanHandle<T>()
    {
        var type = typeof(T);
        
        // Check if type has any of the priority string properties
        foreach (var propertyName in _propertyPriority)
        {
            if (type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.PropertyType == typeof(string))
                return true;
        }
        
        // Check if it has an Id property
        return type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance) != null;
    }
}