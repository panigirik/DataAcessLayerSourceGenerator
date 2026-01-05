using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace DAL.SourceGen.Models;

public sealed record RepositoryMethod
{
    public string MethodName { get; set; }
    public string ReturnedType { get; set; }
    public IReadOnlyList<MethodParameter> Parameters { get; set; }
}