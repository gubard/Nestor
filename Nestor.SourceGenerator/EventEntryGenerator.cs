using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nestor.SourceGenerator;

[Generator]
public class EventEntryGenerator : IIncrementalGenerator
{
    private void CreateIsExists(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            "    public static bool IsExists(global::System.Guid id, global::System.Linq.IQueryable<global::Nestor.Db.EventEntity> events)");

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var isDeleted = events.FirstOrDefault(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}) && x.EntityProperty == \"__IS_DELETED__\");");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(isDeleted is not null)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return isDeleted.EntityBooleanValue is false;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return events.Any(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}));");

        stringBuilder.AppendLine("    }");
    }

    private void CreateIsExistsA(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask}<bool> IsExistsAsync(global::{TypeFullNames.Guid} id, global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} cancellationToken)");

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var isDeleted = await events.FirstOrDefaultAsync(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}) && x.EntityProperty == \"__IS_DELETED__\", cancellationToken);");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(isDeleted is not null)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return isDeleted.EntityBooleanValue is false;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return await events.AnyAsync(x => x.EntityId == id && x.EntityType == nameof(global::{@class.GetFullName()}), cancellationToken);");

        stringBuilder.AppendLine("    }");
    }

    private void CreateDelete(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            $"    public static void Delete{@class.GetName()}s(global::Microsoft.EntityFrameworkCore.DbContext context, params global::System.Guid[] ids)");

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        context.AddRange(ids.Select(x => new global::Nestor.Db.EventEntity {{  EntityId = x, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = \"__IS_DELETED__\", EntityBooleanValue = true }}));");

        stringBuilder.AppendLine("    }");
    }

    private void CreateDeleteA(ClassDeclarationSyntax @class, StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask} Delete{@class.GetName()}sAsync(global::{TypeFullNames.DbContext} context, global::{TypeFullNames.CancellationToken} cancellationToken, params global::{TypeFullNames.Guid}[] ids)");

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        await context.AddRangeAsync(ids.Select(x => new global::{TypeFullNames.EventEntity} {{  EntityId = x, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = \"__IS_DELETED__\", EntityBooleanValue = true }}), cancellationToken);");

        stringBuilder.AppendLine("    }");
    }

    private void CreateGetEntities(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static global::{@class.GetFullName()}[] Get{@class.GetName()}s(global::System.Linq.IQueryable<global::Nestor.Db.EventEntity> events)");

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
                $"events.GroupBy(x => x.EntityId).Select(y => y.Where(x => x.EntityId == y.Key && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id))";

            groupBys[index] = max;
            index++;
        }

        groupBys[groupBys.Length - 1] =
            $"events.GroupBy(x => x.EntityId).Select(y => y.Where(x => x.EntityId == y.Key && x.EntityProperty == \"__IS_DELETED__\" && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id))";

        stringBuilder.AppendLine($"        var query = events.Where(y => {Concat(groupBys)}.Contains(y.Id));");

        stringBuilder.AppendLine(
            "        var properties = query.ToArray().GroupBy(x => x.EntityId).ToDictionary(x => x.Key, x => x.ToDictionary(y => y.EntityProperty));");

        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return properties.Where(x => !x.Value.TryGetValue(\"__IS_DELETED__\", out var isDeleted) || isDeleted.EntityBooleanValue is false).Select(x => new global::{@class.GetFullName()}");

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = x.Key,");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = ({property.Type})x.Value[nameof({property.GetName()})].{GetEntityValueName(property, compilation)},");
        }

        stringBuilder.AppendLine("        }).ToArray();");
        stringBuilder.AppendLine("    }");
    }

    private void CreateGetEntitiesA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async global::{TypeFullNames.ValueTask}<global::{@class.GetFullName()}[]> Get{@class.GetName()}sAsync(global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} cancellationToken)");

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
                $"events.GroupBy(x => x.EntityId).Select(y => y.Where(x => x.EntityId == y.Key && x.EntityProperty == nameof({property.GetName()}) && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id))";

            groupBys[index] = max;
            index++;
        }

        groupBys[groupBys.Length - 1] =
            $"events.GroupBy(x => x.EntityId).Select(y => y.Where(x => x.EntityId == y.Key && x.EntityProperty == \"__IS_DELETED__\" && x.EntityType == nameof(global::{@class.GetFullName()})).Max(x => x.Id))";

        stringBuilder.AppendLine($"        var query = events.Where(y => {Concat(groupBys)}.Contains(y.Id));");

        stringBuilder.AppendLine(
            "        var properties = (await query.ToArrayAsync(cancellationToken)).GroupBy(x => x.EntityId).ToDictionary(x => x.Key, x => x.ToDictionary(y => y.EntityProperty));");

        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            $"        return properties.Where(x => !x.Value.TryGetValue(\"__IS_DELETED__\", out var isDeleted) || isDeleted.EntityBooleanValue is false).Select(x => new global::{@class.GetFullName()}");

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = x.Key,");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = ({property.Type})x.Value[nameof({property.GetName()})].{GetEntityValueName(property, compilation)},");
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
            $"    public static void Edit{@class.GetName()}s(global::Microsoft.EntityFrameworkCore.DbContext context, string userId, global::System.Collections.Generic.IEnumerable<global::{@class.GetNamespace()}.Edit{@class.GetName()}> items)");

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"            if(item.IsEdit{property.GetName()})");
            stringBuilder.AppendLine("            {");

            stringBuilder.AppendLine(
                $"                context.Add(new global::Nestor.Db.EventEntity {{ EntityId = item.{idName}, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = nameof({property.GetName()}), {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()}, UserId = userId}});");

            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
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
            $"    public static async global::{TypeFullNames.ValueTask} Edit{@class.GetName()}s(global::{TypeFullNames.DbContext} context, string userId, global::{TypeFullNames.IEnumerable}<global::{@class.GetNamespace()}.Edit{@class.GetName()}> items, global::{TypeFullNames.CancellationToken} cancellationToken)");

        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"            if(item.IsEdit{property.GetName()})");
            stringBuilder.AppendLine("            {");

            stringBuilder.AppendLine(
                $"                await context.AddAsync(new global::{TypeFullNames.EventEntity} {{ EntityId = item.{idName}, EntityType = nameof(global::{@class.GetFullName()}), EntityProperty = nameof({property.GetName()}), {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()}, UserId = userId}}, cancellationToken);");

            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
    }

    private void CreateFindEntity(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static global::{@class.GetFullName()}? Find{@class.GetName()}(global::System.Guid id, global::System.Linq.IQueryable<global::Nestor.Db.EventEntity> events)");

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
            $"        var query = events.Where(y => y.Id == {string.Join(" || y.Id == ", groupBys.ToArray())});");

        stringBuilder.AppendLine("        var properties = query.ToArray().ToDictionary(x => x.EntityProperty);");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(properties.Count == 0)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            "        if(properties.TryGetValue(\"__IS_DELETED__\", out var isDeleted) && isDeleted.EntityBooleanValue is true)");

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"        return new global::{@class.GetFullName()}");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = id,");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = ({property.Type})properties[nameof({property.GetName()})].{GetEntityValueName(property, compilation)},");
        }

        stringBuilder.AppendLine("        };");
        stringBuilder.AppendLine("    }");
    }
    
 private void CreateFindEntityA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async {TypeFullNames.ValueTask}<global::{@class.GetFullName()}?> Find{@class.GetName()}(global::{TypeFullNames.Guid} id, global::{TypeFullNames.IQueryable}<global::{TypeFullNames.EventEntity}> events, global::{TypeFullNames.CancellationToken} cancellationToken)");

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
            $"        var query = events.Where(y => y.Id == {string.Join(" || y.Id == ", groupBys.ToArray())});");

        stringBuilder.AppendLine("        var properties = (await query.ToArrayAsync(cancellationToken)).ToDictionary(x => x.EntityProperty);");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if(properties.Count == 0)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();

        stringBuilder.AppendLine(
            "        if(properties.TryGetValue(\"__IS_DELETED__\", out var isDeleted) && isDeleted.EntityBooleanValue is true)");

        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return null;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"        return new global::{@class.GetFullName()}");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine($"             {idName} = id,");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine(
                $"             {property.GetName()} = ({property.Type})properties[nameof({property.GetName()})].{GetEntityValueName(property, compilation)},");
        }

        stringBuilder.AppendLine("        };");
        stringBuilder.AppendLine("    }");
    }
 
    private void CreateAddEntity(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static void Add{@class.GetName()}s(global::Microsoft.EntityFrameworkCore.DbContext context, string userId, params global::{@class.GetFullName()}[] items)");

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            "        var events = new global::System.Collections.Generic.List<global::Nestor.Db.EventEntity>();");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine("            events.Add(new global::Nestor.Db.EventEntity");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine($"                EntityId = item.{idName},");
            stringBuilder.AppendLine($"                EntityType = nameof(global::{@class.GetFullName()}),");
            stringBuilder.AppendLine($"                EntityProperty = nameof({property.GetName()}),");

            stringBuilder.AppendLine(
                $"                {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()},");

            stringBuilder.AppendLine("                UserId = userId,");
            stringBuilder.AppendLine("            });");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        context.AddRange(events);");
        stringBuilder.AppendLine("    }");
    }

    private void CreateAddEntityA(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
        Compilation compilation,
        StringBuilder stringBuilder
    )
    {
        stringBuilder.AppendLine(
            $"    public static async {TypeFullNames.ValueTask} Add{@class.GetName()}sAsync(global::{TypeFullNames.DbContext} context, string userId, global::{TypeFullNames.CancellationToken} cancellationToken, params global::{@class.GetFullName()}[] items)");

        stringBuilder.AppendLine("    {");

        stringBuilder.AppendLine(
            $"        var events = new global::{TypeFullNames.List}<global::{TypeFullNames.EventEntity}>();");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        foreach (var item in items)");
        stringBuilder.AppendLine("        {");

        foreach (var property in properties)
        {
            if (idName == property.GetName())
            {
                continue;
            }

            stringBuilder.AppendLine($"            events.Add(new global::{TypeFullNames.EventEntity}");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine($"                EntityId = item.{idName},");
            stringBuilder.AppendLine($"                EntityType = nameof(global::{@class.GetFullName()}),");
            stringBuilder.AppendLine($"                EntityProperty = nameof({property.GetName()}),");

            stringBuilder.AppendLine(
                $"                {GetEntityValueName(property, compilation)} = ({GetEntityTypeName(property.Type, compilation)})item.{property.GetName()},");

            stringBuilder.AppendLine("                UserId = userId,");
            stringBuilder.AppendLine("            });");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        await context.AddRangeAsync(events, cancellationToken);");
        stringBuilder.AppendLine("    }");
    }
    
    private void CreateEditClass(
        string idName,
        ClassDeclarationSyntax @class,
        Span<PropertyDeclarationSyntax> properties,
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
            stringBuilder.AppendLine($"    public {property.Type} {property.GetName()} {{ get; set; }}");
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

            if (symbol is INamedTypeSymbol { TypeKind: TypeKind.Enum, } named)
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

            if (symbol is INamedTypeSymbol { TypeKind: TypeKind.Enum, } named)
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
        var provider = context.SyntaxProvider
           .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax classDeclaration
                 && classDeclaration.AttributeLists
                       .SelectMany(x => x.Attributes)
                       .Any(x => x.Name.ToString() == "SourceEntity"),
                static (context, _) => (ClassDeclarationSyntax)context.Node)
           .Collect();

        var combined = context.CompilationProvider.Combine(provider);

        context.RegisterSourceOutput(combined, (spc, obj) =>
        {
            try
            {
                var (compilation, list) = obj;

                foreach (var source in list)
                {
                    var stringBuilder = new StringBuilder();
                    var properties = source.Members.OfType<PropertyDeclarationSyntax>().ToArray().AsSpan();
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
                    CreateAddEntity(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateAddEntityA(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateFindEntity(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateFindEntityA(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateGetEntities(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateGetEntitiesA(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateIsExists(source, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateIsExistsA(source, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateDelete(source, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateDeleteA(source, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateEditMethod(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine();
                    CreateEditMethodA(idName, source, properties, compilation, stringBuilder);
                    stringBuilder.AppendLine("}");
                    stringBuilder.AppendLine();
                    CreateEditClass(idName, source, properties, stringBuilder);
                    var text = stringBuilder.ToString();
                    spc.AddSource($"EventEntity.{source.GetName()}.g.cs", text);
                }
            }
            catch (Exception e)
            {
                spc.AddSource("EventEntity.g.cs", e.ToString());
            }
        });
    }
}