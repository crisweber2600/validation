namespace Validation.Infrastructure.Services;

using System.Collections.Generic;
using Validation.Domain.Validation;

public interface IValidationPlanProvider
{
    IEnumerable<IValidationRule> GetPlan<T>();
}
