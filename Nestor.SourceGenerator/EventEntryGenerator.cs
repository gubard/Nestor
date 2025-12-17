using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nestor.SourceGenerator;

[Generator]
public class EventEntryGenerator : IIncrementalGenerator
{
    private void CreateIsExistsMethod(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
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

    private void CreateIsExistsMethodA(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
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

    private void CreateDeleteMethod(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            $"    public static void DeleteEntities(global::Microsoft.EntityFrameworkCore.DbContext context, string userId, params global::System.Guid[] ids)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        context.AddRange(ids.Select(x => new global::{TypeFullNames.EventEntity} {{ UserId = userId, IsLast = true, EntityId = x, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = \"__IS_DELETED__\", EntityBooleanValue = true }}));"
        );

        stringBuilder.AppendLine("    }");
    }

    private void CreateDeleteMethodA(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask} DeleteEntitiesAsync(global::{TypeFullNames.DbContext} context, string userId, global::{TypeFullNames.IEnumerable}<global::{TypeFullNames.Guid}> ids, global::{TypeFullNames.CancellationToken} ct)"
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
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static global::{@class.GetFullName()}[] GetEntities(global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events)"
        );

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine(
            "        var properties = events.Where(x => x.IsLast == true).GroupBy(x => x.EntityId).ToArray().ToDictionary(x => x.Key, x => x.ToDictionary(y => y.EntityProperty));"
        );

        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return properties.Where(x => !x.Value.TryGetValue(\"__IS_DELETED__\", out var isDeleted) || isDeleted.EntityBooleanValue is false).Select(x => new global::{@class.GetFullName()}"
        );

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = x.Key,");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = (global::{property.Type.GetFullName(compilation)})x.Value[nameof({property.GetName()})].{GetEntityValueName(property, compilation)},"
            );
        }

        stringBuilder.AppendLine("        }).ToArray();");
        stringBuilder.AppendLine("    }");
    }

    private void CreateGetMethodA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask}<global::{@class.GetFullName()}[]> GetEntitiesAsync(global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine(
            "        var properties = (await events.Where(x => x.IsLast == true).GroupBy(x => x.EntityId).ToArrayAsync(ct)).ToDictionary(x => x.Key, x => x.ToDictionary(y => y.EntityProperty));"
        );

        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return properties.Where(x => !x.Value.TryGetValue(\"__IS_DELETED__\", out var isDeleted) || isDeleted.EntityBooleanValue is false).Select(x => new global::{@class.GetFullName()}"
        );

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = x.Key,");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = (global::{property.Type.GetFullName(compilation)})x.Value[nameof({property.GetName()})].{GetEntityValueName(property, compilation)},"
            );
        }

        stringBuilder.AppendLine("        }).ToArray();");
        stringBuilder.AppendLine("    }");
    }

    private void CreateEditMethod(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static void EditEntities(global::{TypeFullNames.DbContext} context, string userId, global::{@class.GetNamespace()}.Edit{@class.GetName()}[] edits)"
        );

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("    if(edits.Length == 0) { return; }");
        stringBuilder.AppendLine(
            $"    var query = context.Set<global::{TypeFullNames.EventEntity}>().Where(x => x.Id == -1);"
        );
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
                $"            query = query.Concat(query.Where(x => x.IsLast == true && x.EntityId == edit.{idName} && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})));"
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
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask} EditEntitiesAsync(global::{TypeFullNames.DbContext} context, string userId, global::{@class.GetNamespace()}.Edit{@class.GetName()}[] edits, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("    if(edits.Length == 0) { return; }");
        stringBuilder.AppendLine(
            $"    var query = context.Set<global::{TypeFullNames.EventEntity}>().Where(x => x.Id == -1);"
        );
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
                $"            query = query.Concat(query.Where(x => x.IsLast == true && x.EntityId == edit.{idName} && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})));"
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

    private void CreateFindMethod(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static global::{@class.GetFullName()}? FindEntity(global::{TypeFullNames.Guid} id, global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events)"
        );

        stringBuilder.AppendLine("    {");
        var groupBys = new Span<string>(new string[properties.Length]);
        var index = 0;

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            var max =
                $"events.Where(x => x.EntityId == id && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id)";

            groupBys[index] = max;
            index++;
        }

        groupBys[groupBys.Length - 1] =
            $"events.Where(x => x.EntityId == id && x.EntityProperty == \"__IS_DELETED__\" && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id)";

        stringBuilder.AppendLine(
            $"        var query = events.Where(y => y.Id == {string.Join(" || y.Id == ", groupBys.ToArray())});"
        );

        stringBuilder.AppendLine(
            "        var properties = query.ToArray().ToDictionary(x => x.EntityProperty);"
        );
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(properties.Count == 0)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            "        if(properties.TryGetValue(\"__IS_DELETED__\", out var isDeleted) && isDeleted.EntityBooleanValue is true)"
        );

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"        return new global::{@class.GetFullName()}");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = id,");
        var propertyIndex = 0;

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = properties.TryGetValue(nameof({property.GetName()}), out var propertyValue{propertyIndex}) ? (global::{property.Type.GetFullName(compilation)})propertyValue{propertyIndex++}.{GetEntityValueName(property, compilation)} : default(global::{property.Type.GetFullName(compilation)}),"
            );
        }

        stringBuilder.AppendLine("        };");
        stringBuilder.AppendLine("    }");
    }

    private void CreateFindMethodA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async {TypeFullNames.ValueTask}<global::{@class.GetFullName()}?> FindEntityAsync(global::{TypeFullNames.Guid} id, global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} ct)"
        );

        stringBuilder.AppendLine("    {");
        var groupBys = new Span<string>(new string[properties.Length]);
        var index = 0;

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            var max =
                $"events.Where(x => x.EntityId == id && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id)";

            groupBys[index] = max;
            index++;
        }

        groupBys[groupBys.Length - 1] =
            $"events.Where(x => x.EntityId == id && x.EntityProperty == \"__IS_DELETED__\" && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id)";

        stringBuilder.AppendLine(
            $"        var query = events.Where(y => y.Id == {string.Join(" || y.Id == ", groupBys.ToArray())});"
        );

        stringBuilder.AppendLine(
            "        var properties = (await query.ToArrayAsync(ct)).ToDictionary(x => x.EntityProperty);"
        );
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(properties.Count == 0)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            "        if(properties.TryGetValue(\"__IS_DELETED__\", out var isDeleted) && isDeleted.EntityBooleanValue is true)"
        );

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"        return new global::{@class.GetFullName()}");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = id,");
        var propertyIndex = 0;

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = properties.TryGetValue(nameof({property.GetName()}), out var propertyValue{propertyIndex}) ? (global::{property.Type.GetFullName(compilation)})propertyValue{propertyIndex++}.{GetEntityValueName(property, compilation)} : default(global::{property.Type.GetFullName(compilation)}),"
            );
        }

        stringBuilder.AppendLine("        };");
        stringBuilder.AppendLine("    }");
    }

    private void CreateAddMethod(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static void AddEntities(global::Microsoft.EntityFrameworkCore.DbContext context, string userId, params global::{@class.GetFullName()}[] items)"
        );

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var events = new global::System.Collections.Generic.List<global::{TypeFullNames.EventEntity}>();"
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
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async {TypeFullNames.ValueTask} AddEntitiesAsync(global::{TypeFullNames.DbContext} context, string userId, global::{TypeFullNames.IEnumerable}<global::{@class.GetFullName()}> items, global::{TypeFullNames.CancellationToken} ct)"
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
        StringBuilder stringBuilder
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

    private string Concat(Span<string> groupBys)
    {
        if (groupBys.Length == 0)
        {
            return "";
        }

        if (groupBys.Length == 1)
        {
            return groupBys[0];
        }

        return $"{groupBys[0]}.Concat({Concat(groupBys.Slice(1))})";
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
                        var stringBuilder = new StringBuilder();
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
                        CreateFindMethod(idName, source, properties, compilation, stringBuilder);
                        stringBuilder.AppendLine();
                        CreateFindMethodA(idName, source, properties, compilation, stringBuilder);
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
