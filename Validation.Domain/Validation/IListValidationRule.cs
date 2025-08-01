namespace Validation.Domain.Validation;

public interface IListValidationRule<TItem,TKey>
{
    bool Validate(IGrouping<TKey,TItem> duplicates);
}