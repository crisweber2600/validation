using System.Linq.Expressions;
using Validation.Domain.Validation;

namespace Validation.Domain.Metrics;

public record ValidationPlan<T>(Expression<Func<T, double>> Selector, ValidationStrategy Strategy, IValidationRule Rule);
