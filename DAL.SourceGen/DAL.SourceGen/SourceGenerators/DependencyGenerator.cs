using System.Collections.Immutable;
using System.Linq;
using Dal.SourceGen.Abstractions.Attributes;
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
            .CreateSyntaxProvider(static (node, _) => node is InterfaceDeclarationSyntax ids && ids.AttributeLists.Count > 0,
                static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
            .Where(static s => s is not null)
            .Where(HasRepositoryAttribute)
            .Select(static (s, _) => RepositoryGenerator.BuildRepositoryModel(s!))
            .Collect();

        context.RegisterSourceOutput(repos, GenerateDi);
    }

    private static void GenerateDi(
        SourceProductionContext context,
        ImmutableArray<RepositoryModel> repositories)
    {
        var template = Template.Parse("""
                                      using Microsoft.Extensions.DependencyInjection;

                                      public static class AutoDataAccessRegistration
                                      {
                                          public static IServiceCollection AddAutoRepositories(this IServiceCollection services)
                                          {
                                      {{ for r in repositories }}
                                              services.Add{{ r.Lifetime }}<{{ r.InterfaceName }}, {{ r.ImplementationName }}>();
                                      {{ end }}
                                              return services;
                                          }
                                      }
                                      """);

        var code = template.Render(new { repositories }, m => m.Name);
        context.AddSource("AutoDataAccessRegistration.g.cs", code);
    }

    private static bool HasRepositoryAttribute(INamedTypeSymbol symbol)
        => symbol.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(RepositotyAttribute));
}