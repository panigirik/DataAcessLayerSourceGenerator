using System.Collections.Immutable;
using System.Linq;
using DAL.SourceGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;

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
            .Where(static s => s!.GetAttributes()
                .Any(a => a.AttributeClass?.Name == "RepositoryAttribute"))
            .Select(static (s, _) => RepositoryGenerator.BuildModel(s!))
            .Collect();

        context.RegisterSourceOutput(repos, Generate);
    }

    private static void Generate(
        SourceProductionContext ctx,
        ImmutableArray<RepositoryModel> repos)
    {
        var template = Template.Parse("""
                                      using Microsoft.Extensions.DependencyInjection;

                                      public static class AutoDataAccessRegistration
                                      {
                                          public static IServiceCollection AddAutoRepositories(this IServiceCollection services)
                                          {
                                      {{ for r in repos }}
                                              services.Add{{ r.Lifetime }}<{{ r.InterfaceName }}, {{ r.ImplementationName }}>();
                                      {{ end }}
                                              return services;
                                          }
                                      }
                                      """);

        ctx.AddSource("AutoDataAccessRegistration.g.cs", template.Render(new { repos }));
    }
}