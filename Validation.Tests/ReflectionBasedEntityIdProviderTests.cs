using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure;
using Validation.Domain;
using Xunit;

namespace Validation.Tests;

public class ReflectionBasedEntityIdProviderTests
{
    private class TestEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Test";
    }

    [Fact]
    public void GetId_Uses_Priority_String_Properties()
    {
        var entity = new TestEntity { Name = "Foo" };
        var provider = new ReflectionBasedEntityIdProvider();

        var id = provider.GetId(entity);

        Assert.Equal("Foo", id);
    }

    [Fact]
    public void GetId_FallsBack_To_Id_Property()
    {
        var entity = new TestEntity { Name = string.Empty };
        var provider = new ReflectionBasedEntityIdProvider();

        var id = provider.GetId(entity);

        Assert.Equal(entity.Id.ToString(), id);
    }
}
