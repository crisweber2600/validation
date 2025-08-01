using System.ComponentModel.DataAnnotations;

namespace Validation.RepositoryPattern.Sample.Models;

/// <summary>
/// Server entity for demonstrating threshold validation against previously stored values
/// </summary>
public class Server
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public decimal Memory { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsActive { get; set; } = true;

    public void UpdateMemory(decimal newMemory)
    {
        Memory = newMemory;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public override string ToString()
    {
        return $"Server {Name}: {Memory}GB memory";
    }
}