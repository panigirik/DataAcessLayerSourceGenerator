using System.Linq;
using System.Text;
using DAL.SourceGen.Enums;
using DAL.SourceGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DAL.SourceGen.SourceGenerators;

[Generator]
public sealed class RepositoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Генерируем атрибуты
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(
                "RepositoryAttributes.g.cs",
                """
                using System;

                namespace DAL.SourceGen.Attributes;

                public enum ServiceLifetime
                {
                    Singleton = 1,
                    Scoped = 2,
                    Transient = 3
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
                """
            );
        });

        var repos = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InterfaceDeclarationSyntax,
                static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol
            )
            .Where(static s => s is not null)
            .Where(HasRepositoryAttribute)
            .Select(static (s, _) => BuildModel(s!));

        context.RegisterSourceOutput(repos, GenerateImplementation);
    }

    private static bool HasRepositoryAttribute(INamedTypeSymbol symbol)
        => symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "RepositoryAttribute");

    internal static RepositoryModel BuildModel(INamedTypeSymbol symbol)
    {
        var entityType = symbol.Interfaces
            .First(i => i.Name == "IRepository")
            .TypeArguments[0]
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var attr = symbol.GetAttributes()
            .First(a => a.AttributeClass?.Name == "RepositoryAttribute");
        var lifetimeValue = (int)attr.ConstructorArguments[0].Value!;
        var lifetimeEnum = (ServiceLifetime)lifetimeValue;
        string lifetimeMethod = lifetimeEnum switch
        {
            ServiceLifetime.Singleton => "Singleton",
            ServiceLifetime.Scoped => "Scoped",
            ServiceLifetime.Transient => "Transient",
            _ => "Scoped"
        };

        return new RepositoryModel
        {
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            InterfaceName = symbol.Name,
            ImplementationName = symbol.Name.Substring(1),
            EntityType = entityType,
            Lifetime = lifetimeMethod
        };
    }

    private static void GenerateImplementation(SourceProductionContext ctx, RepositoryModel model)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"namespace {model.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"public partial class {model.ImplementationName}");
        sb.AppendLine($"    : BaseRepository<{model.EntityType}>, {model.InterfaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    public {model.ImplementationName}(IAppDbContext db)");
        sb.AppendLine("        : base(db)");
        sb.AppendLine("    { }");
        sb.AppendLine("}");

        ctx.AddSource($"{model.ImplementationName}.g.cs", sb.ToString());
    }
}
