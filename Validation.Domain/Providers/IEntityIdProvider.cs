using System;
using System.Reflection;

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