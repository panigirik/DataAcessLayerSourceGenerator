using DAL.SourceGen.Sample.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.SourceGen.Sample.Repositories;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastracture(this IServiceCollection services)
    {
        services.AddAutoRepositories();
        services.AddScoped<UserService>();
    }
}