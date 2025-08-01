using System.Linq.Expressions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public static class AddValidationFlowsExtensions
{
    public static IServiceCollection AddValidationFlows(
        this IServiceCollection services,
        IEnumerable<ValidationFlowConfig> configs)
    {
        services.AddValidationInfrastructure(cfg =>
        {
            foreach (var flow in configs)
            {
                var type = Type.GetType(flow.Type, true)!;
                if (flow.SaveValidation)
                    cfg.AddConsumer(typeof(SaveValidationConsumer<>).MakeGenericType(type));
                if (flow.SaveCommit)
                    cfg.AddConsumer(typeof(SaveCommitConsumer<>).MakeGenericType(type));
            }
        });

        services.AddSingleton<IValidationPlanProvider>(_ =>
        {
            var provider = new InMemoryValidationPlanProvider();
            foreach (var flow in configs)
            {
                if (flow.MetricProperty != null && flow.ThresholdType.HasValue && flow.ThresholdValue.HasValue)
                {
                    var type = Type.GetType(flow.Type, true)!;
                    var param = Expression.Parameter(typeof(object), "o");
                    var cast = Expression.Convert(param, type);
                    var prop = Expression.Property(cast, flow.MetricProperty);
                    var conv = Expression.Convert(prop, typeof(decimal));
                    var lambda = Expression.Lambda<Func<object, decimal>>(conv, param).Compile();
                    var plan = new ValidationPlan(lambda, flow.ThresholdType.Value, flow.ThresholdValue.Value);
                    typeof(InMemoryValidationPlanProvider).GetMethod("AddPlan")!
                        .MakeGenericMethod(type)
                        .Invoke(provider, new object[] { plan });
                }
            }
            return provider;
        });

        services.AddScoped<SummarisationValidator>();
        return services;
    }
}
