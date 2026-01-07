using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nestor.Db.Helpers;

public static class PropertyBuilderExtension
{
    public static PropertyBuilder<T> SetComparerStruct<T>(this PropertyBuilder<T> propertyBuilder)
        where T : struct
    {
        propertyBuilder.Metadata.SetValueComparer(
            new ValueComparer<T>((c1, c2) => c1.Equals(c2), c => c.GetHashCode(), c => c)
        );

        return propertyBuilder;
    }

    public static PropertyBuilder<T> SetComparerClass<T>(this PropertyBuilder<T> propertyBuilder)
        where T : class
    {
        propertyBuilder.Metadata.SetValueComparer(
            new ValueComparer<T>((c1, c2) => c1 == c2, c => c.GetHashCode(), c => c)
        );

        return propertyBuilder;
    }

    public static PropertyBuilder<T?> SetComparerNullClass<T>(
        this PropertyBuilder<T?> propertyBuilder
    )
        where T : class
    {
        propertyBuilder.Metadata.SetValueComparer(
            new ValueComparer<T?>(
                (c1, c2) => c1 == c2,
                c => c == null ? 0 : c.GetHashCode(),
                c => c
            )
        );

        return propertyBuilder;
    }

    public static PropertyBuilder<T?> SetComparerNullStruct<T>(
        this PropertyBuilder<T?> propertyBuilder
    )
        where T : struct
    {
        propertyBuilder.Metadata.SetValueComparer(
            new ValueComparer<T?>(
                (c1, c2) =>
                    (c1.HasValue || c2.HasValue)
                    && c1.HasValue
                    && c2.HasValue
                    && c1.Value.Equals(c2.Value),
                c => c == null ? 0 : c.GetHashCode(),
                c => c
            )
        );

        return propertyBuilder;
    }
}
