namespace Validation.Domain.Validation;

public class DuplicateEqualityRule<TItem,TKey> : IListValidationRule<TItem,TKey>
{
    public bool Validate(IGrouping<TKey, TItem> duplicates)
    {
        return duplicates.Count() <= 1;
    }
}