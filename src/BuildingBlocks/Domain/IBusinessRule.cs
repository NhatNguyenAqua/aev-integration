namespace Aev.Integration.BuildingBlocks.Domain;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}
