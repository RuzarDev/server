using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
namespace Crm.Presentation.Endpoints;

public static class EndpointExtensions 
{
    public static IServiceCollection AddEndpoints(this IServiceCollection serviceProvider, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) 
                        && t.IsClass 
                        && !t.IsAbstract 
                        && (t.IsPublic || t.IsNestedPublic)                                                                                                                                        
                        && !t.IsGenericTypeDefinition
                        )
            .ToList();
        foreach (var type in types)
        {
            serviceProvider.AddTransient(typeof(IEndpoint),type);
        }
        return serviceProvider;
    }

    public static WebApplication UseEndpoints(this WebApplication app)
    {
       var types =  app.Services.GetServices<IEndpoint>();
       foreach (var type in types)
       {
           type.MapEndpoint(app);
       }
       return app;
    }
}