using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Domain.Repositories;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class GenericRepositoryTests
{
    private class CountingValidator : ISummarisationValidator
    {
        public int Calls { get; private set; }
        public bool Validate(decimal prev, decimal next, IEnumerable<IValidationRule> rules)
        {
            Calls++;
            return true;
        }
    }

    private class EmptyPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => Array.Empty<IValidationRule>();
    }

    [Fact]
    public async Task AddMany_then_SaveChanges_calls_validator_once()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("bulk").Options;
        var context = new TestDbContext(options);
        var validator = new CountingValidator();
        var repo = new EfGenericRepository<Item>(context, validator, new EmptyPlanProvider());

        var items = Enumerable.Range(0, 100).Select(i => new Item(i));
        await repo.AddManyAsync(items);
        await repo.SaveChangesWithPlanAsync();

        Assert.Equal(1, validator.Calls);
        Assert.Equal(100, context.Items.Count());
    }
}
