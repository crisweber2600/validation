using System.Text.Json;
using ValidationFlow.Messages;

namespace Validation.Tests;

public class MessageTests
{
    [Fact]
    public void SaveRequested_round_trips_via_json()
    {
        var message = new SaveRequested<string>("app", "Item", Guid.NewGuid(), "payload");
        var json = JsonSerializer.Serialize(message);
        var result = JsonSerializer.Deserialize<SaveRequested<string>>(json);
        Assert.Equal(message, result);
    }

    [Fact]
    public void SaveValidated_round_trips_via_json()
    {
        var message = new SaveValidated<string>("app", "Item", Guid.NewGuid(), "payload", true);
        var json = JsonSerializer.Serialize(message);
        var result = JsonSerializer.Deserialize<SaveValidated<string>>(json);
        Assert.Equal(message, result);
    }

    [Fact]
    public void DeleteRequested_round_trips_via_json()
    {
        var message = new DeleteRequested<string>("app", "Item", Guid.NewGuid());
        var json = JsonSerializer.Serialize(message);
        var result = JsonSerializer.Deserialize<DeleteRequested<string>>(json);
        Assert.Equal(message, result);
    }
}
