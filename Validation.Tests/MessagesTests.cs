using System.Text.Json;
using ValidationFlow.Messages;
using Xunit;

namespace Validation.Tests;

public class MessagesTests
{
    [Fact]
    public void SaveRequested_serializes_and_compares_by_value()
    {
        var message1 = new SaveRequested<string>("app", "item", "1", "payload");
        var message2 = new SaveRequested<string>("app", "item", "1", "payload");
        Assert.Equal(message1, message2);

        var json = JsonSerializer.Serialize(message1);
        var copy = JsonSerializer.Deserialize<SaveRequested<string>>(json);
        Assert.Equal(message1, copy);
    }

    [Fact]
    public void SaveValidated_serializes_and_compares_by_value()
    {
        var message1 = new SaveValidated<string>("app", "item", "1", "payload", true);
        var message2 = new SaveValidated<string>("app", "item", "1", "payload", true);
        Assert.Equal(message1, message2);

        var json = JsonSerializer.Serialize(message1);
        var copy = JsonSerializer.Deserialize<SaveValidated<string>>(json);
        Assert.Equal(message1, copy);
    }

    [Fact]
    public void SaveCommitFault_serializes_and_compares_by_value()
    {
        var message1 = new SaveCommitFault<string>("app", "item", "1", "payload", "err");
        var message2 = new SaveCommitFault<string>("app", "item", "1", "payload", "err");
        Assert.Equal(message1, message2);

        var json = JsonSerializer.Serialize(message1);
        var copy = JsonSerializer.Deserialize<SaveCommitFault<string>>(json);
        Assert.Equal(message1, copy);
    }

    [Fact]
    public void DeleteRequested_serializes_and_compares_by_value()
    {
        var message1 = new DeleteRequested<string>("app", "item", "1");
        var message2 = new DeleteRequested<string>("app", "item", "1");
        Assert.Equal(message1, message2);

        var json = JsonSerializer.Serialize(message1);
        var copy = JsonSerializer.Deserialize<DeleteRequested<string>>(json);
        Assert.Equal(message1, copy);
    }

    [Fact]
    public void DeleteValidated_serializes_and_compares_by_value()
    {
        var message1 = new DeleteValidated<string>("app", "item", "1");
        var message2 = new DeleteValidated<string>("app", "item", "1");
        Assert.Equal(message1, message2);

        var json = JsonSerializer.Serialize(message1);
        var copy = JsonSerializer.Deserialize<DeleteValidated<string>>(json);
        Assert.Equal(message1, copy);
    }
}
