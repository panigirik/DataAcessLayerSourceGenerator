namespace DAL.SourceGen.Models;

public sealed class RepositoryModel
{
    public string Namespace { get; set; } = default!;
    public string InterfaceName { get; set; } = default!;
    public string ImplementationName { get; set; } = default!;
    public string EntityType { get; set; } = null!;
    public string InterfaceFullName => $"global::{Namespace}.{InterfaceName}";
    public string ImplementationFullName => $"global::{Namespace}.{ImplementationName}";
    public string Lifetime { get; set; }
}
