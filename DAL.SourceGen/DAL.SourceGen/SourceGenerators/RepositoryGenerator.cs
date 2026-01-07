using System.Linq;
using DAL.SourceGen.Enums;
using DAL.SourceGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;

namespace DAL.SourceGen.SourceGenerators;

[Generator]
public sealed class RepositoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 🔹 Генерация атрибутов
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("RepositoryAttributes.g.cs", """
using System;

namespace DAL.SourceGen.Attributes;

public enum ServiceLifetime
{
    Singleton,
    Scoped,
    Transient
}

[AttributeUsage(AttributeTargets.Interface)]
public sealed class RepositoryAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }

    public RepositoryAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}
""");
        });

        var repos = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InterfaceDeclarationSyntax,
                static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol
            )
            .Where(static s => s is not null)
            .Where(HasRepositoryAttribute)
            .Select(static (s, _) => BuildModel(s!));

        context.RegisterSourceOutput(repos, Generate);
    }

    private static bool HasRepositoryAttribute(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name == "RepositoryAttribute");
    }

    internal static RepositoryModel BuildModel(INamedTypeSymbol symbol)
    {
        var entityType = symbol.Interfaces
            .First(i => i.Name == "IRepository")
            .TypeArguments[0]
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var attr = symbol.GetAttributes()
            .First(a => a.AttributeClass?.Name == "RepositoryAttribute");

        return new RepositoryModel
        {
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            InterfaceName = symbol.Name,
            ImplementationName = symbol.Name.Substring(1),
            EntityType = entityType,
            Lifetime = (ServiceLifetime)attr.ConstructorArguments[0].Value!
        };
    }

    private static void Generate(SourceProductionContext ctx, RepositoryModel model)
    {
        var tpl = Template.Parse("""
                                 namespace {{ Namespace }};

                                 public partial class {{ ImplementationName }}
                                     : BaseRepository<{{ EntityType }}>, {{ InterfaceName }}
                                 {
                                     public {{ ImplementationName }}(IAppDbContext db)
                                         : base(db)
                                     {
                                     }
                                 }
                                 """);

        ctx.AddSource($"{model.ImplementationName}.g.cs", tpl.Render(model));
    }

}
