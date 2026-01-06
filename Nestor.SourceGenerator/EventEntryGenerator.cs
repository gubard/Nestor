using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nestor.SourceGenerator;

[Generator]
public class EventEntryGenerator : IIncrementalGenerator
{
    private void CreateDeleteMethod(
        string idName,
        TypeSyntax type,
        ClassDeclarationSyntax @class,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine("public static void DeleteEntities(");
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");
        stringBuilder.AppendLine("params global::System.Guid[] ids)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("if(ids.Length == 0)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("return;");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("var now = DateTimeOffset.UtcNow;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("context.AddRange(ids.Select(x =>");
        stringBuilder.AppendLine($"new global::{TypeFullNames.EventEntity}");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("UserId = userId,");
        stringBuilder.AppendLine("EntityId = x,");
        stringBuilder.AppendLine("TransactionId = transactionId,");
        stringBuilder.AppendLine($"EntityType = nameof(global::{@class.GetFullName()}),");
        stringBuilder.AppendLine("EntityProperty = \"__IS_DELETED__\",");
        stringBuilder.AppendLine("EntityBooleanValue = true,");
        stringBuilder.AppendLine("CreatedAt = now,");
        stringBuilder.AppendLine("}));");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"context.{@class.GetTableName()}");
        stringBuilder.AppendLine($".Where(x => ids.Contains(x.{idName})).ExecuteDelete();");
        stringBuilder.AppendLine("}");
    }

    private void CreateDeleteMethodA(
        string idName,
        TypeSyntax type,
        ClassDeclarationSyntax @class,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"public static {TypeFullNames.ConfiguredValueTaskAwaitable} DeleteEntitiesAsync("
        );
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");
        stringBuilder.AppendLine("global::System.Guid[] ids,");
        stringBuilder.AppendLine($"{TypeFullNames.CancellationToken} ct)");
        stringBuilder.AppendLine("{");

        stringBuilder.AppendLine(
            "return DeleteEntitiesCore(context, userId, transactionId, ids, ct)"
        );

        stringBuilder.AppendLine(".ConfigureAwait(false);");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"private static async {TypeFullNames.ValueTask} DeleteEntitiesCore("
        );

        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");
        stringBuilder.AppendLine("global::System.Guid[] ids,");
        stringBuilder.AppendLine($"{TypeFullNames.CancellationToken} ct)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("if(ids.Length == 0)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("return;");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("var now = DateTimeOffset.UtcNow;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("await context.AddRangeAsync(ids.Select(x =>");
        stringBuilder.AppendLine($"new global::{TypeFullNames.EventEntity}");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("UserId = userId,");
        stringBuilder.AppendLine("EntityId = x,");
        stringBuilder.AppendLine("TransactionId = transactionId,");
        stringBuilder.AppendLine($"EntityType = nameof(global::{@class.GetFullName()}),");
        stringBuilder.AppendLine("EntityProperty = \"__IS_DELETED__\",");
        stringBuilder.AppendLine("EntityBooleanValue = true,");
        stringBuilder.AppendLine("CreatedAt = now,");
        stringBuilder.AppendLine("}), ct);");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"await context.{@class.GetTableName()}");
        stringBuilder.AppendLine($".Where(x => ids.Contains(x.{idName})).ExecuteDeleteAsync(ct);");
        stringBuilder.AppendLine("}");
    }

    private void CreateEditMethod(
        string idName,
        TypeSyntax type,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine("public static void EditEntities(");
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");

        stringBuilder.AppendLine(
            $"params global::{@class.GetNamespace()}.Edit{@class.GetName()}[] edits)"
        );

        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("if(edits.Length == 0)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("return;");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"var ids = edits.Select(x => x.{idName}).Distinct().ToArray();");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"var entities = context.{@class.GetTableName()}");
        stringBuilder.AppendLine($".Where(x => ids.Contains(x.{idName}))");
        stringBuilder.AppendLine($".ToDictionary(x => x.{idName}).ToFrozenDictionary();");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("var index = 0;");
        stringBuilder.AppendLine("var eventCount = edits.Sum(x => x.GetEdited());");
        stringBuilder.AppendLine("var now = DateTimeOffset.UtcNow;");

        stringBuilder.AppendLine(
            $"var events = new global::{TypeFullNames.EventEntity}[eventCount];"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("foreach (var edit in edits)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine($"var entity = entities[edit.{idName}];");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"if(edit.IsEdit{property.GetName()})");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"entity.{property.GetName()} = edit.{property.GetName()};");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"events[index++] = new global::{TypeFullNames.EventEntity}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"EntityId = edit.{idName},");
            stringBuilder.AppendLine($"EntityType = nameof(global::{@class.GetFullName()}),");
            stringBuilder.AppendLine($"EntityProperty = nameof({property.GetName()}),");
            stringBuilder.AppendLine(
                $"{GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})edit.{property.GetName()},"
            );
            stringBuilder.AppendLine("UserId = userId,");
            stringBuilder.AppendLine("TransactionId = transactionId,");
            stringBuilder.AppendLine("CreatedAt = now,");
            stringBuilder.AppendLine("};");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("context.AddRange(events);");
        stringBuilder.AppendLine("}");
    }

    private void CreateEditMethodA(
        string idName,
        TypeSyntax type,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"public static {TypeFullNames.ConfiguredValueTaskAwaitable} EditEntitiesAsync("
        );
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");

        stringBuilder.AppendLine(
            $"global::{@class.GetNamespace()}.Edit{@class.GetName()}[] edits,"
        );

        stringBuilder.AppendLine($"global::{TypeFullNames.CancellationToken} ct)");
        stringBuilder.AppendLine("{");

        stringBuilder.AppendLine(
            "return EditEntitiesCore(context, userId, transactionId, edits, ct)"
        );

        stringBuilder.AppendLine(".ConfigureAwait(false);");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(
            $"private static async {TypeFullNames.ValueTask} EditEntitiesCore("
        );
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");

        stringBuilder.AppendLine(
            $"global::{@class.GetNamespace()}.Edit{@class.GetName()}[] edits,"
        );

        stringBuilder.AppendLine($"global::{TypeFullNames.CancellationToken} ct)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("if(edits.Length == 0)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("return;");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"var ids = edits.Select(x => x.{idName}).Distinct().ToArray();");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"var entities = (await context.{@class.GetTableName()}");
        stringBuilder.AppendLine($".Where(x => ids.Contains(x.{idName}))");
        stringBuilder.AppendLine($".ToDictionaryAsync(x => x.{idName})).ToFrozenDictionary();");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("var index = 0;");
        stringBuilder.AppendLine("var eventCount = edits.Sum(x => x.GetEdited());");
        stringBuilder.AppendLine("var now = DateTimeOffset.UtcNow;");

        stringBuilder.AppendLine(
            $"var events = new global::{TypeFullNames.EventEntity}[eventCount];"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("foreach (var edit in edits)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine($"var entity = entities[edit.{idName}];");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"if(edit.IsEdit{property.GetName()})");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"entity.{property.GetName()} = edit.{property.GetName()};");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"events[index++] = new global::{TypeFullNames.EventEntity}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"EntityId = edit.{idName},");
            stringBuilder.AppendLine($"EntityType = nameof(global::{@class.GetFullName()}),");
            stringBuilder.AppendLine($"EntityProperty = nameof({property.GetName()}),");
            stringBuilder.AppendLine(
                $"{GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})edit.{property.GetName()},"
            );
            stringBuilder.AppendLine("UserId = userId,");
            stringBuilder.AppendLine("TransactionId = transactionId,");
            stringBuilder.AppendLine("CreatedAt = now,");
            stringBuilder.AppendLine("};");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("await context.AddRangeAsync(events, ct);");
        stringBuilder.AppendLine("}");
    }

    private void CreateAddMethod(
        string idName,
        TypeSyntax type,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine("public static void AddEntities(");
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");
        stringBuilder.AppendLine($"params global::{@class.GetFullName()}[] items)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("var index = 0;");

        stringBuilder.AppendLine(
            $"var events = new {TypeFullNames.EventEntity}[items.Length * {properties.Length - 1}];"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("foreach (var item in items)");
        stringBuilder.AppendLine("{");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"events[index++] = new global::{TypeFullNames.EventEntity}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"EntityId = item.{idName},");
            stringBuilder.AppendLine($"EntityType = nameof(global::{@class.GetFullName()}),");
            stringBuilder.AppendLine($"EntityProperty = nameof({property.GetName()}),");

            stringBuilder.AppendLine(
                $"{GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()},"
            );

            stringBuilder.AppendLine("UserId = userId,");
            stringBuilder.AppendLine("TransactionId = transactionId,");
            stringBuilder.AppendLine("};");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("context.AddRange(events);");
        stringBuilder.AppendLine("context.AddRange(items);");
        stringBuilder.AppendLine("}");
    }

    private void CreateAddMethodA(
        string idName,
        TypeSyntax type,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"public static {TypeFullNames.ConfiguredValueTaskAwaitable} AddEntitiesAsync("
        );
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");
        stringBuilder.AppendLine($"global::{@class.GetFullName()}[] items,");
        stringBuilder.AppendLine($"{TypeFullNames.CancellationToken} ct)");
        stringBuilder.AppendLine("{");

        stringBuilder.AppendLine(
            "return AddEntitiesCore(context, userId, transactionId, items, ct)"
        );

        stringBuilder.AppendLine(".ConfigureAwait(false);");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(
            $"private static async {TypeFullNames.ValueTask} AddEntitiesCore("
        );
        stringBuilder.AppendLine($"global::{type.GetFullName(compilation)} context,");
        stringBuilder.AppendLine("string userId,");
        stringBuilder.AppendLine("Guid transactionId,");
        stringBuilder.AppendLine($"global::{@class.GetFullName()}[] items,");
        stringBuilder.AppendLine($"{TypeFullNames.CancellationToken} ct)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("var index = 0;");

        stringBuilder.AppendLine(
            $"var events = new {TypeFullNames.EventEntity}[items.Length * {properties.Length - 1}];"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("foreach (var item in items)");
        stringBuilder.AppendLine("{");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"events[index++] = new global::{TypeFullNames.EventEntity}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"EntityId = item.{idName},");
            stringBuilder.AppendLine($"EntityType = nameof(global::{@class.GetFullName()}),");
            stringBuilder.AppendLine($"EntityProperty = nameof({property.GetName()}),");

            stringBuilder.AppendLine(
                $"{GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()},"
            );

            stringBuilder.AppendLine("UserId = userId,");
            stringBuilder.AppendLine("TransactionId = transactionId,");
            stringBuilder.AppendLine("};");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("await context.AddRangeAsync(events, ct);");
        stringBuilder.AppendLine("await context.AddRangeAsync(items, ct);");
        stringBuilder.AppendLine("}");
    }

    private void CreateEditClass(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine($"public class Edit{@class.GetName()}");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine($"public Edit{@class.GetName()}(global::System.Guid id)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine($"{idName} = id;");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"public global::System.Guid {idName} {{ get; }}");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"public bool IsEdit{property.GetName()} {{ get; set; }}");
            stringBuilder.AppendLine(
                $"    public {property.Type.GetFullName(compilation)} {property.GetName()} {{ get; set; }}"
            );
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("public int GetEdited()");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("var count = 0;");
        stringBuilder.AppendLine();

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"if(IsEdit{property.GetName()})");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("count++;");
            stringBuilder.AppendLine("}");
        }
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("return count;");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine("}");
    }

    private string GetEntityValueName(PropertyDeclarationSyntax property, Compilation compilation)
    {
        return GetEntityValueName(property.Type, compilation);
    }

    private string GetEntityValueName(TypeSyntax type, Compilation compilation)
    {
        if (type is ArrayTypeSyntax array)
        {
            return $"Entity{array.ElementType.GetRealName()}ArrayValue";
        }

        if (type is IdentifierNameSyntax identifier)
        {
            var semanticModel = compilation.GetSemanticModel(identifier.SyntaxTree);
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);
            var symbol = symbolInfo.Symbol;

            if (symbol is INamedTypeSymbol { TypeKind: TypeKind.Enum } named)
            {
                return GetEntityValueName(named.EnumUnderlyingType);
            }
        }

        if (type is NullableTypeSyntax nullable)
        {
            return GetEntityValueName(nullable.ElementType, compilation);
        }

        return $"Entity{type.GetRealName()}Value";
    }

    private string GetEntityValueName(INamedTypeSymbol named)
    {
        return $"Entity{named.Name}Value";
    }

    private string GetEntityTypeName(TypeSyntax type, Compilation compilation)
    {
        if (type is IdentifierNameSyntax identifier)
        {
            var semanticModel = compilation.GetSemanticModel(identifier.SyntaxTree);
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);
            var symbol = symbolInfo.Symbol;

            if (symbol is INamedTypeSymbol { TypeKind: TypeKind.Enum } named)
            {
                return GetEntityTypeName(named.EnumUnderlyingType);
            }
        }

        return type.ToString();
    }

    private string GetEntityTypeName(INamedTypeSymbol named)
    {
        return named.Name;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, _) =>
                    node is ClassDeclarationSyntax classDeclaration
                    && classDeclaration
                        .AttributeLists.SelectMany(x => x.Attributes)
                        .Any(x => x.Name.ToString() == "SourceEntity"),
                static (context, _) => (ClassDeclarationSyntax)context.Node
            )
            .Collect();

        var combined = context.CompilationProvider.Combine(provider);

        context.RegisterSourceOutput(
            combined,
            (spc, obj) =>
            {
                try
                {
                    var (compilation, list) = obj;

                    foreach (var source in list)
                    {
                        var stringBuilder = new CSharpStringBuilder(4);

                        var properties = source
                            .Members.OfType<PropertyDeclarationSyntax>()
                            .ToArray()
                            .AsSpan();

                        var idName = source.GetAttributeValueSting("SourceEntity", 0);
                        var type = source.GetAttributeValueType("SourceEntity", 1);

                        if (idName == null)
                        {
                            return;
                        }

                        stringBuilder.AppendLine("// <auto-generated />");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("#nullable enable");
                        stringBuilder.AppendLine("#pragma warning disable CS8601");
                        stringBuilder.AppendLine("#pragma warning disable CS8629");
                        stringBuilder.AppendLine("#pragma warning disable CS8600");
                        stringBuilder.AppendLine("#pragma warning disable CS8618");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("using System.Linq;");
                        stringBuilder.AppendLine("using System.Collections.Frozen;");
                        stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore;");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine($"namespace {source.GetNamespace()};");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine($"partial class {source.GetName()}");
                        stringBuilder.AppendLine("{");

                        CreateAddMethod(
                            idName,
                            type,
                            source,
                            properties,
                            compilation,
                            stringBuilder
                        );

                        stringBuilder.AppendLine();

                        CreateAddMethodA(
                            idName,
                            type,
                            source,
                            properties,
                            compilation,
                            stringBuilder
                        );

                        stringBuilder.AppendLine();

                        CreateDeleteMethod(idName, type, source, compilation, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateDeleteMethodA(idName, type, source, compilation, stringBuilder);
                        stringBuilder.AppendLine();

                        CreateEditMethod(
                            idName,
                            type,
                            source,
                            properties,
                            compilation,
                            stringBuilder
                        );

                        stringBuilder.AppendLine();

                        CreateEditMethodA(
                            idName,
                            type,
                            source,
                            properties,
                            compilation,
                            stringBuilder
                        );

                        stringBuilder.AppendLine("}");
                        stringBuilder.AppendLine();
                        CreateEditClass(idName, source, properties, compilation, stringBuilder);
                        var text = stringBuilder.ToString();
                        spc.AddSource($"EventEntity.{source.GetName()}.g.cs", text);
                    }
                }
                catch (Exception e)
                {
                    spc.AddSource("EventEntity.g.cs", e.ToString());
                }
            }
        );
    }
}
