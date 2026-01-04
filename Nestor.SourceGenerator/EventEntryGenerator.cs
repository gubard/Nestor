using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nestor.SourceGenerator;

[Generator]
public class EventEntryGenerator : IIncrementalGenerator
{
    private void CreateIsExistsMethod(
        ClassDeclarationSyntax @class,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static bool IsEntityExists(global::System.Guid id, global::System.Linq.IQueryable<global::{TypeFullNames.EventEntity}> events)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var isDeleted = events.FirstOrDefault(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}) && x.EntityProperty == \"__IS_DELETED__\");"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(isDeleted is not null)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return isDeleted.EntityBooleanValue is false;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return events.Any(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}));"
        );

        stringBuilder.AppendLine("    }");
    }

    private void CreateIsExistsMethodA(
        ClassDeclarationSyntax @class,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask}<bool> IsEntityExistsAsync(global::{TypeFullNames.Guid} id, global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var isDeleted = await events.FirstOrDefaultAsync(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}) && x.EntityProperty == \"__IS_DELETED__\", ct);"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(isDeleted is not null)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return isDeleted.EntityBooleanValue is false;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return await events.AnyAsync(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}), ct);"
        );

        stringBuilder.AppendLine("    }");
    }

    private void CreateDeleteMethod(
        ClassDeclarationSyntax @class,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static void DeleteEntities(global::{TypeFullNames.NestorDbContext} context, string userId, params global::System.Guid[] ids)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        context.AddRange(ids.Select(x => new global::{TypeFullNames.EventEntity} {{ UserId = userId, IsLast = true, EntityId = x, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = \"__IS_DELETED__\", EntityBooleanValue = true }}));"
        );

        stringBuilder.AppendLine("    }");
    }

    private void CreateDeleteMethodA(
        ClassDeclarationSyntax @class,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask} DeleteEntitiesAsync(global::{TypeFullNames.NestorDbContext} context, string userId, global::{TypeFullNames.IEnumerable}<global::{TypeFullNames.Guid}> ids, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        await context.AddRangeAsync(ids.Select(x => new global::{TypeFullNames.EventEntity} {{ UserId = userId, IsLast = true, EntityId = x, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = \"__IS_DELETED__\", EntityBooleanValue = true }}), ct);"
        );

        stringBuilder.AppendLine("    }");
    }

    private void CreateGetMethod(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static global::{@class.GetFullName()}[] GetEntities(global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events)"
        );

        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine(
            "var deletedIds = events.Where(x => x.IsLast == true && x.EntityProperty == \"__IS_DELETED__\" && x.EntityBooleanValue == true).Select(x => x.EntityId);"
        );
        stringBuilder.AppendLine(
            "var rawEntities = events.Where(x => x.IsLast == true && !deletedIds.Contains(x.EntityId)).GroupBy(x => x.EntityId).ToArray();"
        );
        stringBuilder.AppendLine(
            $"var entities = new global::{@class.GetFullName()}[rawEntities.Length];"
        );
        stringBuilder.AppendLine("for (var index = 0; index < rawEntities.Length; index++)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("var rawEntity = rawEntities[index];");
        stringBuilder.AppendLine(
            $"var entity = new global::{@class.GetFullName()} {{ {idName} = rawEntity.Key }};"
        );
        stringBuilder.AppendLine("entities[index] = entity;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("foreach (var property in rawEntity)");
        stringBuilder.AppendLine("switch (property.EntityProperty)");
        stringBuilder.AppendLine("{");

        foreach (var property in properties)
        {
            stringBuilder.AppendLine(
                $"case nameof(global::{@class.GetFullName()}.{property.GetName()}):"
            );
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(
                $"entity.{property.GetName()} = (global::{property.Type.GetFullName(compilation)})property.{GetEntityValueName(property, compilation)};"
            );
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("break;");
            stringBuilder.AppendLine("}");
        }

        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine("}");

        stringBuilder.AppendLine("return entities;");
        stringBuilder.AppendLine("}");
    }

    private void CreateGetMethodA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"public static async global::{TypeFullNames.ValueTask}<global::{@class.GetFullName()}[]> GetEntitiesAsync(global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine(
            "var deletedIds = events.Where(x => x.IsLast == true && x.EntityProperty == \"__IS_DELETED__\" && x.EntityBooleanValue == true).Select(x => x.EntityId);"
        );
        stringBuilder.AppendLine(
            "var rawEntities = await events.Where(x => x.IsLast == true && !deletedIds.Contains(x.EntityId)).GroupBy(x => x.EntityId).ToArrayAsync(ct);"
        );
        stringBuilder.AppendLine(
            $"var entities = new global::{@class.GetFullName()}[rawEntities.Length];"
        );
        stringBuilder.AppendLine("for (var index = 0; index < rawEntities.Length; index++)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("var rawEntity = rawEntities[index];");
        stringBuilder.AppendLine(
            $"var entity = new global::{@class.GetFullName()} {{ {idName} = rawEntity.Key }};"
        );
        stringBuilder.AppendLine("entities[index] = entity;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("foreach (var property in rawEntity)");
        stringBuilder.AppendLine("switch (property.EntityProperty)");
        stringBuilder.AppendLine("{");

        foreach (var property in properties)
        {
            stringBuilder.AppendLine(
                $"case nameof(global::{@class.GetFullName()}.{property.GetName()}):"
            );
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(
                $"entity.{property.GetName()} = (global::{property.Type.GetFullName(compilation)})property.{GetEntityValueName(property, compilation)};"
            );
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("break;");
            stringBuilder.AppendLine("}");
        }

        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine("}");

        stringBuilder.AppendLine("return entities;");
        stringBuilder.AppendLine("}");
    }

    private void CreateEditMethod(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static void EditEntities(global::{TypeFullNames.NestorDbContext} context, string userId, global::{@class.GetNamespace()}.Edit{@class.GetName()}[] edits)"
        );

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("    if(edits.Length == 0) { return; }");
        stringBuilder.AppendLine("    var query = context.Events.Where(x => x.Id == -1);");
        stringBuilder.AppendLine(
            $"        var events = new global::{TypeFullNames.List}<global::{TypeFullNames.EventEntity}>();"
        );
        stringBuilder.AppendLine("        foreach (var edit in edits)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"            if(edit.IsEdit{property.GetName()})");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine(
                $"            query = query.Concat(context.Events.Where(x => x.IsLast == true && x.EntityId == edit.{idName} && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})));"
            );
            stringBuilder.AppendLine(
                $"                events.Add(new global::{TypeFullNames.EventEntity} {{ EntityId = edit.{idName}, IsLast = true, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = nameof({property.GetName()}), {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})edit.{property.GetName()}, UserId = userId}});"
            );

            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("        var items = query.ToArray();");
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            item.IsLast = false;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("        context.AddRange(events);");
        stringBuilder.AppendLine("    }");
    }

    private void CreateEditMethodA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask} EditEntitiesAsync(global::{TypeFullNames.NestorDbContext} context, string userId, global::{@class.GetNamespace()}.Edit{@class.GetName()}[] edits, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("    if(edits.Length == 0) { return; }");
        stringBuilder.AppendLine("    var query = context.Events.Where(x => x.Id == -1);");
        stringBuilder.AppendLine(
            $"        var events = new global::{TypeFullNames.List}<global::{TypeFullNames.EventEntity}>();"
        );
        stringBuilder.AppendLine("        foreach (var edit in edits)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"            if(edit.IsEdit{property.GetName()})");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine(
                $"            query = query.Concat(context.Events.Where(x => x.IsLast == true && x.EntityId == edit.{idName} && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})));"
            );
            stringBuilder.AppendLine(
                $"                events.Add(new global::{TypeFullNames.EventEntity} {{ EntityId = edit.{idName}, IsLast = true, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = nameof({property.GetName()}), {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})edit.{property.GetName()}, UserId = userId}});"
            );

            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("        var items = await query.ToArrayAsync(ct);");
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            item.IsLast = false;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("        await context.AddRangeAsync(events, ct);");
        stringBuilder.AppendLine("    }");
    }

    private void CreateFindMethod(ClassDeclarationSyntax @class, CSharpStringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            $"public static global::{@class.GetFullName()}? FindEntity(global::{TypeFullNames.Guid} id, global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events)"
        );
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine(
            "return GetEntities(events.Where(x => x.EntityId == id)).FirstOrDefault();"
        );
        stringBuilder.AppendLine("}");
    }

    private void CreateFindMethodA(ClassDeclarationSyntax @class, CSharpStringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            $"    public static async {TypeFullNames.ValueTask}<global::{@class.GetFullName()}?> FindEntityAsync(global::{TypeFullNames.Guid} id, global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} ct)"
        );
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine(
            "return (await GetEntitiesAsync(events.Where(x => x.EntityId == id), ct)).FirstOrDefault();"
        );
        stringBuilder.AppendLine("}");
    }

    private void CreateAddMethod(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static void AddEntities(global::{TypeFullNames.NestorDbContext} context, string userId, params global::{@class.GetFullName()}[] items)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var events = new {TypeFullNames.List}<global::{TypeFullNames.EventEntity}>();"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"            events.Add(new global::{TypeFullNames.EventEntity}"
            );
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine($"                EntityId = item.{idName},");
            stringBuilder.AppendLine(
                $"                EntityType = nameof(global::{@class.GetFullName()}),"
            );
            stringBuilder.AppendLine($"                IsLast = true,");
            stringBuilder.AppendLine(
                $"                EntityProperty = nameof({property.GetName()}),"
            );

            stringBuilder.AppendLine(
                $"                {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()},"
            );

            stringBuilder.AppendLine("                UserId = userId,");
            stringBuilder.AppendLine("            });");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        context.AddRange(events);");
        stringBuilder.AppendLine("    }");
    }

    private void CreateAddMethodA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        CSharpStringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async {TypeFullNames.ValueTask} AddEntitiesAsync(global::{TypeFullNames.NestorDbContext} context, string userId, global::{TypeFullNames.IEnumerable}<global::{@class.GetFullName()}> items, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var events = new global::{TypeFullNames.List}<global::{TypeFullNames.EventEntity}>();"
        );

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"            events.Add(new global::{TypeFullNames.EventEntity}"
            );
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine($"                EntityId = item.{idName},");
            stringBuilder.AppendLine(
                $"                EntityType = nameof(global::{@class.GetFullName()}),"
            );
            stringBuilder.AppendLine($"                IsLast = true,");
            stringBuilder.AppendLine(
                $"                EntityProperty = nameof({property.GetName()}),"
            );

            stringBuilder.AppendLine(
                $"                {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()},"
            );

            stringBuilder.AppendLine("                UserId = userId,");
            stringBuilder.AppendLine("            });");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        await context.AddRangeAsync(events, ct);");
        stringBuilder.AppendLine("    }");
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
        stringBuilder.AppendLine($"    public Edit{@class.GetName()}(global::System.Guid id)");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine($"          {idName} = id;");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"    public global::System.Guid {idName} {{ get; }}");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"    public bool IsEdit{property.GetName()} {{ get; set; }}");
            stringBuilder.AppendLine(
                $"    public {property.Type.GetFullName(compilation)} {property.GetName()} {{ get; set; }}"
            );
        }

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
                        CreateAddMethod(idName, source, properties, compilation, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateAddMethodA(idName, source, properties, compilation, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateFindMethod(source, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateFindMethodA(source, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateGetMethod(idName, source, properties, compilation, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateGetMethodA(idName, source, properties, compilation, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateIsExistsMethod(source, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateIsExistsMethodA(source, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateDeleteMethod(source, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateDeleteMethodA(source, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateEditMethod(idName, source, properties, compilation, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateEditMethodA(idName, source, properties, compilation, stringBuilder);
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
