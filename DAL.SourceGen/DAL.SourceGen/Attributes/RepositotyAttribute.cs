using System;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.SourceGen.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public class RepositotyAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }

    public RepositotyAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}