using System.Linq;
using DAL.SourceGen.Attributes;
using DAL.SourceGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Scriban;

namespace DAL.SourceGen.SourceGenerators;

[Generator]
public sealed class RepositoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var repoInterfaces = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InterfaceDeclarationSyntax ids && ids.AttributeLists.Count > 0,
                static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol
            )
            .Where(static s => s is not null)
            .Where(HasRepositoryAttribute);

        var models = repoInterfaces
            .Select(static (s, _) => BuildRepositoryModel(s!));

        context.RegisterSourceOutput(models, GenerateRepository);
    }

    public static RepositoryModel BuildRepositoryModel(INamedTypeSymbol symbol)
    {
        var methods = symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary)
            .Select(m => new RepositoryMethod
            {
                MethodName = m.Name,
                ReturnedType = m.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Parameters = m.Parameters
                    .Select(p => new MethodParameter
                    {
                        Name = p.Name,
                        Type = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    })
                    .ToList()
            })
            .ToList();

        var attr = symbol.GetAttributes()
            .First(a => a.AttributeClass!.Name == nameof(RepositotyAttribute));

        return new RepositoryModel
        {
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            InterfaceName = symbol.Name,
            ImplementationName = symbol.Name.Substring(1),
            EntityType = "UNKNOWN_FOR_NOW",
            Lifetime = (ServiceLifetime)attr.ConstructorArguments[0].Value!,
            Methods = methods
        };
    }

    private static void GenerateRepository(SourceProductionContext context, RepositoryModel model)
    {
        var template = Template.Parse("""
namespace {{ Namespace }};

public partial class {{ ImplementationName }} : {{ InterfaceName }}
{
{{ for m in Methods }}
    public {{ m.ReturnedType }} {{ m.MethodName }}(
{{ for p in m.Parameters }}
        {{ p.Type }} {{ p.Name }}{{ if !for.last }},{{ end }}
{{ end }}
    )
    {
        throw new System.NotImplementedException();
    }
{{ end }}
}
""");

        var code = template.Render(model, m => m.Name);
        context.AddSource($"{model.ImplementationName}.g.cs", code);
    }

    private static bool HasRepositoryAttribute(INamedTypeSymbol symbol)
        => symbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name == nameof(RepositotyAttribute));
}
