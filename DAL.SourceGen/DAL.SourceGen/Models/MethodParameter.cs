namespace DAL.SourceGen.Models;

public sealed record MethodParameter
{
    public string Name { get; set; }
    public string Type { get; set; }
}