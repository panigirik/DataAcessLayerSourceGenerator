using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DAL.SourceGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DAL.SourceGen.SourceGenerators;

[Generator]
public sealed class DependencyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var repos = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InterfaceDeclarationSyntax,
                static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol
            )
            .Where(static s => s is not null)
            .Where(static s => s!.GetAttributes().Any(a => a.AttributeClass?.Name == "RepositoryAttribute"))
            .Select(static (s, _) => RepositoryGenerator.BuildModel(s!))
            .Collect();

        context.RegisterSourceOutput(repos, GenerateRegistration);
    }

    private static void GenerateRegistration(SourceProductionContext ctx, ImmutableArray<RepositoryModel> repos)
    {
        if (repos.IsDefaultOrEmpty) return;

        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        
        foreach (var ns in repos.Select(r => r.Namespace).Distinct())
        {
            sb.AppendLine($"using {ns};");
        }

        sb.AppendLine();
        sb.AppendLine("public static class AutoDataAccessRegistration");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddAutoRepositories(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var r in repos)
        {
            sb.AppendLine($"        services.Add{r.Lifetime}<{r.InterfaceFullName}, {r.ImplementationFullName}>();");
        }

        sb.AppendLine();
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource("AutoDataAccessRegistration.g.cs", sb.ToString());
    }
}
