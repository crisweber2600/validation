using System.Text.Json;

namespace Validation.Infrastructure.DI;

public class ValidationFlowOptions
{
    public List<ValidationFlowDefinition> Flows { get; set; } = new();

    public static ValidationFlowOptions Load(string json)
    {
        return JsonSerializer.Deserialize<ValidationFlowOptions>(json) ?? new ValidationFlowOptions();
    }
}
