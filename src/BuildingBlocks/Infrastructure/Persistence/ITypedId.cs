namespace Aev.Integration.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Marker interface for strongly-typed ID value objects.
/// Implement this interface on record structs to enable automatic EF Core value conversion.
/// </summary>
/// <typeparam name="TPrimitive">The underlying primitive type (e.g., Guid, int, string).</typeparam>
public interface ITypedId<out TPrimitive>
{
    TPrimitive Value { get; }
}
