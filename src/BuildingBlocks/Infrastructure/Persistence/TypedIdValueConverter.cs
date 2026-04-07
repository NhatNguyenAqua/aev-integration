using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// EF Core value converter for strongly-typed IDs that implement <see cref="ITypedId{TPrimitive}"/>.
/// Converts between the typed ID and its underlying primitive type for database storage.
/// </summary>
/// <typeparam name="TTypedId">The strongly-typed ID type (e.g., a record struct).</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type stored in the database.</typeparam>
public class TypedIdValueConverter<TTypedId, TPrimitive>(
    Func<TPrimitive, TTypedId> fromPrimitive,
    Func<TTypedId, TPrimitive> toPrimitive,
    ConverterMappingHints? mappingHints = null)
    : ValueConverter<TTypedId, TPrimitive>(
        typedId => toPrimitive(typedId),
        primitive => fromPrimitive(primitive),
        mappingHints)
{
}
