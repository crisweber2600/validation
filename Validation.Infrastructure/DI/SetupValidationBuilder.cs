using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.DI;

public enum AuditRepositoryType
{
    EfCore,
    Mongo
}

public class SetupValidationBuilder
{
    public AuditRepositoryType RepositoryType { get; private set; } = AuditRepositoryType.EfCore;

    public SetupValidationBuilder UseMongo()
    {
        RepositoryType = AuditRepositoryType.Mongo;
        return this;
    }

    public SetupValidationBuilder UseEntityFramework()
    {
        RepositoryType = AuditRepositoryType.EfCore;
        return this;
    }

    internal void Apply(IServiceCollection services)
    {
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        services.AddScoped<SummarisationValidator>();
    }
}
