using System.Text.Json;
using ValidationFlow.Messages;

namespace Validation.Tests;

public class MessagesTests
{
    private record Payload(int Value);

    [Fact]
    public void SaveRequested_serializes_and_compares()
    {
        var message = new SaveRequested<Payload>("app", "item", Guid.NewGuid(), new Payload(1));
        var clone = JsonSerializer.Deserialize<SaveRequested<Payload>>(JsonSerializer.Serialize(message));

        Assert.Equal(message, clone);
    }

    [Fact]
    public void SaveValidated_serializes_and_compares()
    {
        var message = new SaveValidated<Payload>("app", "item", Guid.NewGuid(), new Payload(2), true);
        var clone = JsonSerializer.Deserialize<SaveValidated<Payload>>(JsonSerializer.Serialize(message));

        Assert.Equal(message, clone);
    }

    [Fact]
    public void SaveCommitFault_serializes_and_compares()
    {
        var message = new SaveCommitFault<Payload>("app", "item", Guid.NewGuid(), new Payload(3), "error");
        var clone = JsonSerializer.Deserialize<SaveCommitFault<Payload>>(JsonSerializer.Serialize(message));

        Assert.Equal(message, clone);
    }

    [Fact]
    public void DeleteRequested_serializes_and_compares()
    {
        var message = new DeleteRequested("app", "item", Guid.NewGuid());
        var clone = JsonSerializer.Deserialize<DeleteRequested>(JsonSerializer.Serialize(message));

        Assert.Equal(message, clone);
    }

    [Fact]
    public void DeleteValidated_serializes_and_compares()
    {
        var message = new DeleteValidated("app", "item", Guid.NewGuid());
        var clone = JsonSerializer.Deserialize<DeleteValidated>(JsonSerializer.Serialize(message));

        Assert.Equal(message, clone);
    }
}

