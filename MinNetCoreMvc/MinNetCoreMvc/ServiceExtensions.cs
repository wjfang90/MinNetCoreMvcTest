using Microsoft.Extensions.DependencyInjection.Extensions;
using MinNetCoreMvc.Attributes;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public static class ServiceExtensions {

        public static IServiceCollection AddMinControllers(this IServiceCollection services) {
            services.TryAddSingleton<IMinActionInvokerFactory, MinActionInvokerFactory>();
            services.TryAddSingleton<IMinActionMethodExecutor, MinActionMethodExecutor>();
            services.TryAddSingleton<IMinActionResultConverter, MinActionResultConverter>();
            services.TryAddSingleton<IMinArgumentBinder, MinArgumentBinder>();
            services.TryAddSingleton<IMinActionDescriptorCollectionProvider, MinActionDescriptorCollectionProvider>();
            return services;
        }

        public static IEndpointConventionBuilder MapMinControllerRoute(this IEndpointRouteBuilder builder, string name, [StringSyntax("Route")] string pattern, object? defaults = null, object? constraints = null, object? dataTokens = null) {
            var source = new MinActionEndpointDataSource(builder.ServiceProvider);
            builder.DataSources.Add(source);
            return source.AddRoute(name, pattern, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), new RouteValueDictionary(dataTokens));
        }
    }
}
