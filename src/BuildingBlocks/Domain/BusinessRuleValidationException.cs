namespace Aev.Integration.BuildingBlocks.Domain;

public class BusinessRuleValidationException(IBusinessRule brokenRule)
    : Exception(brokenRule.Message)
{
    public IBusinessRule BrokenRule { get; } = brokenRule;

    public override string ToString() =>
        $"Business rule '{BrokenRule.GetType().Name}' violation: {BrokenRule.Message}";
}
