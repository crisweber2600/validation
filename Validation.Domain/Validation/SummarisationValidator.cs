namespace Validation.Domain.Validation;

public class SummarisationValidator
{
    public bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules)
    {
        return rules.All(r => r.Validate(previousValue, newValue));
    }

    public bool Validate<TItem,TKey>(IEnumerable<TItem> items,
        Func<TItem,TKey> keySelector,
        IEnumerable<IListValidationRule<TItem,TKey>> rules)
    {
        var groups = items.GroupBy(keySelector);
        foreach (var group in groups)
        {
            foreach (var rule in rules)
            {
                if (!rule.Validate(group))
                    return false;
            }
        }

        return true;
    }
}