using System;
using System.ComponentModel.DataAnnotations;
using Validation.Domain.Events;

namespace Validation.Domain.Entities;

public class NannyRecord : BaseEntity
{
    
    [Required]
    public string Name { get; private set; } = string.Empty;
    
    public string? ContactInfo { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    
    public DateTime? LastModified { get; private set; }
    
    public NannyRecord(string name, string? contactInfo = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
            
        Name = name;
        ContactInfo = contactInfo;
        AddEvent(new SaveRequested(Id));
    }

    public void UpdateContactInfo(string contactInfo)
    {
        ContactInfo = contactInfo;
        LastModified = DateTime.UtcNow;
        AddEvent(new SaveRequested(Id));
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
            
        Name = name;
        LastModified = DateTime.UtcNow;
        AddEvent(new SaveRequested(Id));
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModified = DateTime.UtcNow;
        AddEvent(new DeleteRequested(Id));
    }

    public void Reactivate()
    {
        IsActive = true;
        LastModified = DateTime.UtcNow;
        AddEvent(new SaveRequested(Id));
    }
}