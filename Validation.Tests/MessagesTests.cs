using System.Text.Json;
using ValidationFlow.Messages;

namespace Validation.Tests;

public class MessagesTests
{
    [Fact]
    public void SaveRequested_equality_and_serialization()
    {
        var id = Guid.NewGuid();
        var m1 = new SaveRequested<string>("app", "entity", id, "payload");
        var m2 = new SaveRequested<string>("app", "entity", id, "payload");
        Assert.Equal(m1, m2);
        var json = JsonSerializer.Serialize(m1);
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void SaveValidated_equality_and_serialization()
    {
        var id = Guid.NewGuid();
        var m1 = new SaveValidated<string>("app", "entity", id, "payload", true);
        var m2 = new SaveValidated<string>("app", "entity", id, "payload", true);
        Assert.Equal(m1, m2);
        var json = JsonSerializer.Serialize(m1);
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void SaveCommitFault_equality_and_serialization()
    {
        var id = Guid.NewGuid();
        var m1 = new SaveCommitFault<string>("app", "entity", id, "payload", "error");
        var m2 = new SaveCommitFault<string>("app", "entity", id, "payload", "error");
        Assert.Equal(m1, m2);
        var json = JsonSerializer.Serialize(m1);
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void DeleteRequested_equality_and_serialization()
    {
        var id = Guid.NewGuid();
        var m1 = new DeleteRequested<string>("app", "entity", id);
        var m2 = new DeleteRequested<string>("app", "entity", id);
        Assert.Equal(m1, m2);
        var json = JsonSerializer.Serialize(m1);
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void DeleteValidated_equality_and_serialization()
    {
        var id = Guid.NewGuid();
        var m1 = new DeleteValidated<string>("app", "entity", id, true);
        var m2 = new DeleteValidated<string>("app", "entity", id, true);
        Assert.Equal(m1, m2);
        var json = JsonSerializer.Serialize(m1);
        Assert.False(string.IsNullOrWhiteSpace(json));
    }
}
