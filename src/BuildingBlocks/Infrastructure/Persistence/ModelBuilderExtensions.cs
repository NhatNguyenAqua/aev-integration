using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures a typed ID property with its value converter explicitly.
    /// </summary>
    public static PropertyBuilder<TTypedId> HasTypedIdConversion<TTypedId, TPrimitive>(
        this PropertyBuilder<TTypedId> builder,
        Func<TPrimitive, TTypedId> fromPrimitive,
        Func<TTypedId, TPrimitive> toPrimitive)
    {
        return builder.HasConversion(new TypedIdValueConverter<TTypedId, TPrimitive>(fromPrimitive, toPrimitive));
    }

    /// <summary>
    /// Scans all entity types and automatically applies <see cref="TypedIdValueConverter{TTypedId, TPrimitive}"/>
    /// for any property whose CLR type implements <see cref="ITypedId{TPrimitive}"/>.
    /// Call this at the end of <c>OnModelCreating</c>.
    /// </summary>
    public static ModelBuilder ApplyTypedIdConversions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var clrType = property.ClrType;
                var typedIdInterface = clrType
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypedId<>));

                if (typedIdInterface is null)
                    continue;

                var primitiveType = typedIdInterface.GetGenericArguments()[0];
                var converterType = typeof(TypedIdValueConverter<,>).MakeGenericType(clrType, primitiveType);

                var fromPrimitiveParam = System.Linq.Expressions.Expression.Parameter(primitiveType);
                var ctor = clrType.GetConstructor([primitiveType]);
                if (ctor is null)
                    continue;

                var fromPrimitiveLambda = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.New(ctor, fromPrimitiveParam),
                    fromPrimitiveParam);

                var toPrimitiveParam = System.Linq.Expressions.Expression.Parameter(clrType);
                var valueProperty = clrType.GetProperty(nameof(ITypedId<object>.Value));
                if (valueProperty is null)
                    continue;

                var toPrimitiveLambda = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Property(toPrimitiveParam, valueProperty),
                    toPrimitiveParam);

                var converter = (ValueConverter)Activator.CreateInstance(
                    converterType,
                    fromPrimitiveLambda.Compile(),
                    toPrimitiveLambda.Compile(),
                    null)!;

                property.SetValueConverter(converter);
            }
        }

        return modelBuilder;
    }
}
