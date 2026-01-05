using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.SourceGen.Models;

public sealed record RepositoryModel
{
    public string Namespace { get; set; }
    public string InterfaceName { get; set; }
    public string ImplementationName { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public ServiceLifetime Lifetime { get; set; }
    public IReadOnlyList<RepositoryMethod> Methods { get; set; } = null!;
}