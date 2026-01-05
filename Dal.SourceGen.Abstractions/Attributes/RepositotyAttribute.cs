using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dal.SourceGen.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public abstract class RepositotyAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public RepositotyAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}