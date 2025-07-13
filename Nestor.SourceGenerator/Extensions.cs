using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nestor.SourceGenerator;

public static class Extensions
{
    public static string GetAttributeValueSting<T>(this T syntax, string attributeName, int argumentIndex)
        where T : BaseTypeDeclarationSyntax
    {
        var attribute = syntax.AttributeLists
           .SelectMany(x => x.Attributes)
           .First(x => x.Name.ToString() == attributeName);

        return attribute.ArgumentList?.Arguments[argumentIndex].Expression switch
        {
            InvocationExpressionSyntax invocation => invocation.ArgumentList.Arguments[0].ToString(),
            { } e => e.ToString(),
        };
    }

    public static string GetNamespace<T>(this T syntax) where T : SyntaxNode
    {
        return syntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().First().Name.ToString();
    }

    public static string GetName<T>(this T syntax) where T : BaseTypeDeclarationSyntax
    {
        return syntax.Identifier.Text;
    }

    public static string GetFullName<T>(this T syntax) where T : BaseTypeDeclarationSyntax
    {
        return $"{syntax.GetNamespace()}.{syntax.GetName()}";
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
}