using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nestor.SourceGenerator;

public static class Extensions
{
    public static string GetEntityTypeFullName(this IPropertySymbol property)
    {
        if (
            property.Type is INamedTypeSymbol
            {
                TypeKind: TypeKind.Enum,
                EnumUnderlyingType: not null
            } named
        )
        {
            return $"(global::{named.EnumUnderlyingType.GetRealFullName()})";
        }

        return string.Empty;
    }

    public static string GetEntityValueName(this IPropertySymbol property)
    {
        return GetEntityValueName(property.Type);
    }

    private static string GetEntityValueName(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { Name: "Nullable" } named)
        {
            return GetEntityValueName(named.TypeArguments[0]);
        }

        if (type is IArrayTypeSymbol array)
        {
            return $"Entity{array.ElementType.GetRealName()}ArrayValue";
        }

        if (type is INamedTypeSymbol { TypeKind: TypeKind.Enum, EnumUnderlyingType: not null } e)
        {
            return $"Entity{e.EnumUnderlyingType.GetRealName()}Value";
        }

        return $"Entity{type.GetRealName()}Value";
    }

    public static TypeSyntax GetAttributeValueType<T>(
        this T syntax,
        string attributeName,
        int argumentIndex
    )
        where T : BaseTypeDeclarationSyntax
    {
        var attribute = syntax
            .AttributeLists.SelectMany(x => x.Attributes)
            .First(x => x.Name.ToString() == attributeName);

        return attribute.ArgumentList?.Arguments[argumentIndex].Expression switch
        {
            TypeOfExpressionSyntax typeOf => typeOf.Type,
            _ => throw new InvalidOperationException(),
        };
    }

    public static string GetAttributeValueSting<T>(
        this T syntax,
        string attributeName,
        int argumentIndex
    )
        where T : BaseTypeDeclarationSyntax
    {
        var attribute = syntax
            .AttributeLists.SelectMany(x => x.Attributes)
            .First(x => x.Name.ToString() == attributeName);

        return attribute.ArgumentList?.Arguments[argumentIndex].Expression switch
        {
            InvocationExpressionSyntax invocation => invocation
                .ArgumentList.Arguments[0]
                .ToString(),
            { } e => e.ToString(),
        };
    }

    public static string GetNamespace(this ISymbol symbol)
    {
        return symbol.ContainingNamespace.ToDisplayString();
    }

    public static string GetNamespace<T>(this T syntax)
        where T : SyntaxNode
    {
        return syntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().First().Name.ToString();
    }

    public static string GetName<T>(this T syntax)
        where T : BaseTypeDeclarationSyntax
    {
        return syntax.Identifier.Text;
    }

    public static string GetTableName<T>(this T syntax)
        where T : BaseTypeDeclarationSyntax
    {
        var entityName = syntax.GetName();

        return $"{entityName.Substring(0, entityName.Length - 6)}s";
    }

    public static string GetTableName(this ISymbol syntax)
    {
        return $"{syntax.Name.Substring(0, syntax.Name.Length - 6)}s";
    }

    public static string GetFullName<T>(this T syntax)
        where T : BaseTypeDeclarationSyntax
    {
        return $"{syntax.GetNamespace()}.{syntax.GetName()}";
    }

    public static string GetFullName(this TypeSyntax syntax, Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
        var typeInfo = semanticModel.GetTypeInfo(syntax);

        return $"{typeInfo.Type?.ContainingNamespace.ToDisplayString()}.{syntax.GetRealName()}";
    }

    public static string GetName(this PropertyDeclarationSyntax syntax)
    {
        return syntax.Identifier.Text;
    }

    public static string GetRealName(this TypeSyntax syntax)
    {
        return syntax switch
        {
            PredefinedTypeSyntax p => p.Keyword.Text switch
            {
                "bool" => nameof(Boolean),
                "sbyte" => nameof(SByte),
                "short" => nameof(Int16),
                "int" => nameof(Int32),
                "long" => nameof(Int64),
                "byte" => nameof(Byte),
                "ushort" => nameof(UInt16),
                "uint" => nameof(UInt32),
                "ulong" => nameof(UInt64),
                "float" => nameof(Single),
                "double" => nameof(Double),
                "decimal" => nameof(Decimal),
                "char" => nameof(Char),
                "string" => nameof(String),
                { } s => s,
            },
            { } s => s.ToString(),
        };
    }

    public static string GetRealFullName(this ISymbol symbol)
    {
        return symbol.ToString() switch
        {
            "bool" => "System." + nameof(Boolean),
            "sbyte" => "System." + nameof(SByte),
            "short" => "System." + nameof(Int16),
            "int" => "System." + nameof(Int32),
            "long" => "System." + nameof(Int64),
            "byte" => "System." + nameof(Byte),
            "ushort" => "System." + nameof(UInt16),
            "uint" => "System." + nameof(UInt32),
            "ulong" => "System." + nameof(UInt64),
            "float" => "System." + nameof(Single),
            "double" => "System." + nameof(Double),
            "decimal" => "System." + nameof(Decimal),
            "char" => "System." + nameof(Char),
            "string" => "System." + nameof(String),
            "byte[]" => "System." + nameof(Byte) + "[]",
            { } s => s,
        };
    }

    public static string GetRealName(this ISymbol symbol)
    {
        return symbol.ToString() switch
        {
            "bool" => nameof(Boolean),
            "sbyte" => nameof(SByte),
            "short" => nameof(Int16),
            "int" => nameof(Int32),
            "long" => nameof(Int64),
            "byte" => nameof(Byte),
            "ushort" => nameof(UInt16),
            "uint" => nameof(UInt32),
            "ulong" => nameof(UInt64),
            "float" => nameof(Single),
            "double" => nameof(Double),
            "decimal" => nameof(Decimal),
            "char" => nameof(Char),
            "string" => nameof(String),
            { } s => symbol.Name,
        };
    }
}
