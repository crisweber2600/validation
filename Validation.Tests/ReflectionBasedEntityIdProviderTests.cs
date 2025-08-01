using System;
using Validation.Infrastructure;

namespace Validation.Tests;

public class ReflectionBasedEntityIdProviderTests
{
    private class TestEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = "ABC";
        public string Name { get; set; } = "Test";
    }

    [Fact]
    public void GetId_prioritizes_configured_properties()
    {
        var entity = new TestEntity { Code = "XYZ", Name = "Foo" };
        var provider = new ReflectionBasedEntityIdProvider("Code", "Name");

        var id = provider.GetId(entity);
        Assert.Equal("XYZ", id);
    }

    [Fact]
    public void GetId_falls_back_to_Id_property()
    {
        var entity = new TestEntity { Code = string.Empty, Name = string.Empty };
        var provider = new ReflectionBasedEntityIdProvider();

        var id = provider.GetId(entity);
        Assert.Equal(entity.Id.ToString(), id);
    }
}
